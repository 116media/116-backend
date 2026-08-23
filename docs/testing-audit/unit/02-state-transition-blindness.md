# Critical — State transitions are never asserted

35 handlers in `src/` invoke a domain state-transition method on an entity. 24 of
their matching test files never assert the state that resulted. The tests assert
that the handler returned, that `Update` was called, and that the transaction was
committed — none of which changes if the transition itself is deleted.

## The problem

`AdminPublishArticleHandler` is the canonical shape. The transition is one line,
and the return value carries no information about it:

```csharp
// src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/PublishArticle/AdminPublishArticleHandler.cs:48-52
article.Publish();
articleRepository.Update(article: article);
await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

return new AdminPublishArticleResult(IsSuccess: true);
```

The only success test for that handler asserts three things, none of which is the
publication:

```csharp
// tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/PublishArticle/AdminPublishArticleHandlerTests.cs:40-55
[Fact]
public async Task Handle_WhenArticleIsApproved_ShouldPublishAndReturnSuccess()
{
    // Arrange
    ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
    var command = new AdminPublishArticleCommand(Id: article.Id.ToString());
    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    // Act
    AdminPublishArticleResult result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    _articleRepositoryMock.VerifyUpdateCalled();
    _unitOfWorkMock.VerifyCommitCalled();
}
```

Take the three assertions in turn. `IsSuccess` is a hard-coded `true` on the result
record, so it is a constant (see
[01-assertions-that-cannot-fail.md](01-assertions-that-cannot-fail.md)).
`VerifyCommitCalled()` proves a transaction was opened and closed. And
`VerifyUpdateCalled()` does not look at what was updated:

```csharp
// tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:148-151
public static void VerifyUpdateCalled(this Mock<IArticleRepository> mock)
{
    mock.Verify(x => x.Update(It.IsAny<ArticleEntity>()), Times.Once);
}
```

`It.IsAny<ArticleEntity>()` accepts the article in whatever state it happens to be
in. Delete the `article.Publish();` line from the handler and every assertion in the
test still holds: the repository is still handed an `ArticleEntity`, the unit of
work still commits, and the result record still hard-codes `IsSuccess: true`. The
test is green and the article is still in `Approved`.

### The same shape, twenty-four times

The rejection handler is worse, because it writes two fields and the test asserts
neither, while holding a live reference to the entity:

```csharp
// src/.../RejectArticle/AdminRejectArticleHandler.cs:48
article.Reject(reason: command.Reason);
```

```csharp
// tests/Unit/.../RejectArticle/AdminRejectArticleHandlerTests.cs:42-59
ArticleEntity article = ArticleFactory.CreatePendingReview(CategoryId);
var command = new AdminRejectArticleCommand(
    Id: article.Id.ToString(),
    Reason: TestConstants.Content.Editorial.Article.ValidRejectionReason
);
_articleRepositoryMock.SetupGetByIdOrThrow(article);

AdminRejectArticleResult result = await _handler.Handle(command, CancellationToken.None);

result.IsSuccess.Should().BeTrue();
_articleRepositoryMock.VerifyUpdateCalled();
_unitOfWorkMock.VerifyCommitCalled();
```

`ArticleEntity.Reject(string reason)` sets `Status` and `RejectionReason`
(`src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs:475-485`). The
variable `article` is in scope on the line after the call. Asserting
`article.Status` and `article.RejectionReason` costs two lines and is not done.

The archive handler adds a further gap. `ArticleEntity.Archive()` returns a `bool`
signalling whether the transition happened, and the handler branches on it:

```csharp
// src/.../ArchiveArticle/AdminArchiveArticleHandler.cs:35-39
bool archived = article.Archive();

if (!archived)
{
    throw i18n.Article.AlreadyArchived();
}
```

`AdminArchiveArticleHandlerTests` asserts `IsSuccess`, `VerifyUpdateCalled()` and
`VerifyCommitCalled()` and nothing else, so neither the archived status nor the
meaning of the `false` branch is covered by state.

The full list of handlers that invoke a transition and whose tests never assert the
result: `AdminPublishArticleHandler`, `AdminApproveArticleHandler`,
`AdminRejectArticleHandler`, `AdminArchiveArticleHandler`,
`AdminSubmitArticleHandler`, `AdminPublishVideoHandler`,
`AdminApproveVideoHandler`, `AdminRejectVideoHandler`,
`AdminArchiveVideoHandler`, `AdminSubmitVideoHandler`,
`AdminPublishLyricsHandler`, `AdminApproveLyricsHandler`,
`AdminRejectLyricsHandler`, `AdminArchiveLyricsHandler`,
`AdminSubmitLyricsHandler`, `AdminApproveLyricsSubmissionHandler`,
`AdminActivateCategoryHandler`, `AdminDeactivateCategoryHandler`,
`AdminActivatePackageHandler`, `AdminDeactivatePackageHandler`,
`AdminActivateShortVideoHandler`, `AdminDeactivateShortVideoHandler`,
`AdminCancelOrderHandler`, `AdminRejectPaymentHandler`.

Across the whole unit suite, of 337 handler test files only 28 contain a
`.Status.Should()` assertion and only 8 mention `DomainEvents` at all.

## Why it matters

A state machine is the part of a content system most likely to be got wrong and
most expensive to get wrong. Publication controls what the public API serves;
rejection controls whether an editor sees a reason; archival controls whether a
record is still reachable. All three are protected by a test that would pass with
the transition removed.

The concrete failure mode is a refactor. Someone reorders the handler, moves the
`Publish()` call inside a branch that no longer executes, or replaces the entity
method with one that silently no-ops on an unexpected status. The unit suite stays
green, because it was only ever checking that the plumbing ran. The bug surfaces as
"the editor pressed publish and nothing happened", after release.

The mirror-image failure is a false negative on the guard clauses. `Publish()`,
`Reject()` and `Archive()` each return `bool` or throw, and each has a no-op branch
for the already-in-that-state case. Nothing in these tests distinguishes "the
transition ran" from "the transition declined to run", so the no-op branch and the
success branch are indistinguishable to the suite.

## The fix

Assert the entity state and the domain events. The entity is already in scope, so
this is additive:

```csharp
// Before
[Fact]
public async Task Handle_WhenArticleIsApproved_ShouldPublishAndReturnSuccess()
{
    ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
    var command = new AdminPublishArticleCommand(Id: article.Id.ToString());
    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    AdminPublishArticleResult result = await _handler.Handle(command, CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    _articleRepositoryMock.VerifyUpdateCalled();
    _unitOfWorkMock.VerifyCommitCalled();
}
```

```csharp
// After
[Fact]
public async Task Handle_WhenArticleIsApproved_ShouldTransitionToPublished()
{
    ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
    var command = new AdminPublishArticleCommand(Id: article.Id.ToString());
    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    await _handler.Handle(command, CancellationToken.None);

    article.Status.Should().Be(EnumContentStatus.Published);
    article.PublishedAt.Should().NotBeNull();
    _articleRepositoryMock.VerifyUpdateCalled(article);
    _unitOfWorkMock.VerifyCommitCalled();
}
```

For rejection, the reason is part of the transition and belongs in the assertion:

```csharp
[Fact]
public async Task Handle_WhenArticleInPendingReview_ShouldRecordStatusAndReason()
{
    ArticleEntity article = ArticleFactory.CreatePendingReview(CategoryId);
    const string reason = "Sources are not verifiable.";
    var command = new AdminRejectArticleCommand(Id: article.Id.ToString(), Reason: reason);
    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    await _handler.Handle(command, CancellationToken.None);

    article.Status.Should().Be(EnumContentStatus.Rejected);
    article.RejectionReason.Should().Be(reason);
    _articleRepositoryMock.VerifyUpdateCalled(article);
}
```

### Change the mock helper to check identity

`VerifyUpdateCalled()` should take the entity it expects, so the assertion proves
*which* article was handed to the repository rather than that some article was:

```csharp
// Before — tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:148-151
public static void VerifyUpdateCalled(this Mock<IArticleRepository> mock)
{
    mock.Verify(x => x.Update(It.IsAny<ArticleEntity>()), Times.Once);
}
```

```csharp
// After
public static void VerifyUpdateCalled(this Mock<IArticleRepository> mock, ArticleEntity expected)
{
    mock.Verify(x => x.Update(expected), Times.Once);
}
```

The neighbouring `VerifyRemoveCalled(this Mock<IArticleRepository> mock, ArticleEntity article)`
at line 153 already takes the entity, so the pattern is established in the same
file; `VerifyUpdateCalled` is the outlier.

### Two correct examples already in the suite

Identity's activation handler test asserts the transition on the entity it
supplied:

```csharp
// tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/ActivateRole/AdminActivateRoleHandlerTests.cs:80
inactiveRole.IsActive.Should().BeTrue();
```

`ContentOrderEntityTests` is the model to copy for domain-level coverage. It splits
the transition from the event, asserts the event payload by value, and — critically
— asserts that the failure path raises nothing:

```csharp
// tests/Unit/Modules/Content/Domain/Entities/ContentOrderEntityTests.cs:112-147
[Fact]
public void Submit_WhenDraft_ShouldTransitionToPendingPayment()
{
    ContentOrderEntity order = ContentOrderFactory.Create();

    order.Submit(_errors);

    order.Status.Should().Be(EnumOrderStatus.PendingPayment);
}

[Fact]
public void Submit_WhenDraft_ShouldRaiseOrderSubmittedEvent()
{
    ContentOrderEntity order = ContentOrderFactory.Create();

    order.Submit(_errors);

    order
        .DomainEvents.OfType<OrderSubmittedEvent>()
        .Should()
        .ContainSingle()
        .Which.Should()
        .Be(new OrderSubmittedEvent(order.Id));
}

[Fact]
public void Submit_WhenNotDraft_ShouldThrowConflictException()
{
    ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
    order.ClearDomainEvents();

    Action act = () => order.Submit(_errors);

    act.Should().Throw<ConflictException>();
    order.DomainEvents.Should().BeEmpty();
}
```

The `order.DomainEvents.Should().BeEmpty()` on the failure path is the assertion
that the 24 blind handler tests most conspicuously lack: it proves the rejected
transition did not half-happen.

## The principle

**A test for a state transition must name the state.** The handler's job is to move
an entity from one status to another; if the test does not mention the destination
status, it is not testing the handler's job.

Three rules follow:

1. **Assert the post-state on the entity the test supplied.** The mock returns an
   entity the test constructed, so the test holds the reference and can read it
   after the call. There is no reason to go through the mock to find out what
   happened.
2. **Assert the domain events, including their absence.** A transition that raises
   an event and a transition that silently mutates are different behaviours; only
   `DomainEvents` distinguishes them. On the guard-clause path, assert the
   collection is empty.
3. **Never verify a mutation with `It.IsAny<T>()`.** If the point of the call is
   that a specific object was handed over in a specific state, the verification must
   name that object.

## Checklist

- [ ] The success test asserts the destination status by enum value, not `IsSuccess`.
- [ ] Every field the transition writes (`RejectionReason`, `PublishedAt`,
      `DeletedAt`, `ArchivedAt`) is asserted alongside the status.
- [ ] Domain events raised by the transition are asserted by payload value.
- [ ] The failure and no-op paths assert `DomainEvents.Should().BeEmpty()` and that
      the status is unchanged.
- [ ] `Verify(...Update(...))` names the expected entity instead of `It.IsAny<T>()`.
- [ ] Deleting the transition call from the handler makes the test fail. If it does
      not, the test is not finished.
