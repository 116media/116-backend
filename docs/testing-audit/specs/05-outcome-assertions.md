# Spec 05 — Outcome assertions

> **Status: landed.** Changes 1, 3, 4, 5 and 6 are in the code and grep-verified.
> Change 2 is reduced but was not re-audited site by site, so its box alone stays
> unticked. Change 4 converted all 33 numeric-literal sites:
> `grep -rn "BeGreaterThanOrEqualTo" tests/` returns one result, the `TimeSpan.Zero`
> duration assertion this change exempts. See the implementation notes below and
> [00-index.md](00-index.md) for global progress.

## Goal

The unit suite asserts that handlers returned, that mocks were called, and that
non-nullable values are not null. It rarely asserts what the code under test
actually produced. This spec replaces six families of assertion that cannot fail
with assertions that name a specific change to `src/` that would break them: state
transitions and domain events in the 24 blind handler tests, `BeOfType<T>` on
`T`-typed variables, `NotBeNull` on non-nullable values, inequality assertions
against arranged counts, query-builder tests that never evaluate a predicate, and
six tests that assert nothing at all. The governing question for every edit here is
the one from [../standards/03-assertion-catalogue.md](../standards/03-assertion-catalogue.md):
would this test still pass if the method body were `return default;`?

## Scope

In scope:

- `VerifyUpdateCalled` in the repository mock helpers, changed to take the expected
  entity.
- The 24 handler test files whose only outcome assertion is `IsSuccess`.
- The 112 compiler-guaranteed `BeOfType<T>` sites.
- The 234 `Should().NotBeNull()` assertions on values that cannot be null,
  concentrated in the three `DbContext` test files and the specification tests.
- The 33 `BeGreaterThanOrEqualTo(n)` sites with a numeric literal.
- The 5 query-builder test files that assert only `NotBeNull` / `BeNull` on
  `Build()`.
- The 6 tests with no assertion.

Not in this spec:

- The 104 localization self-comparison tests. They are spec 06.
- Mock argument matching (`It.IsAny` → `It.Is<T>`), blanket mock defaults, and dead
  helper deletion. Those are spec 07, and this spec deliberately touches
  `VerifyUpdateCalled` only, leaving the rest of the mock surface alone.
- Error-response assertions (`ShouldBeProblem`), `BeOneOf` on status codes, and
  `Throw<Exception>()`. Those are spec 04.
- Collapsing the resulting near-duplicate tests into theories. That is spec 10,
  and it runs after this one so that it collapses assertions that already mean
  something.
- Any change to `src/`. The two production defects the query-builder work touches
  are recorded in spec 13 and fixed there, not here.

## Prerequisites

- **Spec 01 (test host fidelity)** and **spec 02 (test isolation)** must land
  first. Change 4 replaces tolerance with exact counts, and an exact count is only
  correct once the database reset and the stub reset contracts hold. Running change
  4 before them converts a real isolation defect into a wall of red that looks like
  an assertion problem.
- **Spec 04 (error assertion discipline)** is not a hard dependency but should
  land first where the two overlap in the same integration file, so that a file is
  edited once rather than twice.

## Changes

### 1. Assert the state transition and the domain events in handler tests

**Files:** `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs`, plus the
24 handler test files listed in
[../unit/02-state-transition-blindness.md](../unit/02-state-transition-blindness.md).

Start with the mock helper, because every one of the 24 files calls it.
`VerifyUpdateCalled` currently accepts any entity in any state:

```csharp
// tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:148-151 — before
public static void VerifyUpdateCalled(this Mock<IArticleRepository> mock)
{
    mock.Verify(x => x.Update(It.IsAny<ArticleEntity>()), Times.Once);
}
```

```csharp
// after
/// <summary>
/// Verifies that the repository was handed <paramref name="expected" /> exactly once.
/// Naming the entity is what distinguishes "the handler updated the article it looked
/// up" from "the handler updated some article", which is the only version of this
/// assertion a wrong-entity bug can fail.
/// </summary>
/// <param name="mock">The repository mock under verification.</param>
/// <param name="expected">The entity the handler is expected to have updated.</param>
public static void VerifyUpdateCalled(this Mock<IArticleRepository> mock, ArticleEntity expected)
{
    mock.Verify(x => x.Update(expected), Times.Once);
}
```

The neighbouring `VerifyRemoveCalled(this Mock<IArticleRepository> mock, ArticleEntity article)`
at `MockArticleRepository.cs:153` already takes the entity, so the shape is
established in the same file. Apply the identical change to the `VerifyUpdateCalled`
overloads on the video, lyrics, category, package, short-video, order and payment
repository mocks under `tests/Unit/Common/Mocks/Repositories/`.

*If done wrong:* leaving a parameterless overload in place lets the 24 call sites
compile unchanged and the whole change silently does nothing. Delete the
parameterless form rather than overloading it.

Then rewrite each handler success test to assert the destination state. The
publish handler is the canonical case. `AdminPublishArticleHandler.cs:48-52` calls
`article.Publish()`, and `ArticleEntity.Publish()` at
`src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs:445-465` sets
`Status`, stamps `PublishedAt`, and raises both `CommissionedContentPublishedEvent`
and `ArticlePublishedEvent`. None of that is asserted:

```csharp
// tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/PublishArticle/AdminPublishArticleHandlerTests.cs:40-54 — before
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

```csharp
// after
[Fact]
public async Task Handle_WhenArticleIsApproved_ShouldTransitionToPublished()
{
    // Arrange
    ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
    var command = new AdminPublishArticleCommand(Id: article.Id.ToString());
    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    article.Status.Should().Be(EnumContentStatus.Published);
    article.PublishedAt.Should().NotBeNull();
    _articleRepositoryMock.VerifyUpdateCalled(article);
    _unitOfWorkMock.VerifyCommitCalled();
}

[Fact]
public async Task Handle_WhenArticleIsApproved_ShouldRaiseArticlePublishedEvent()
{
    // Arrange
    ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
    var command = new AdminPublishArticleCommand(Id: article.Id.ToString());
    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    article
        .DomainEvents.OfType<ArticlePublishedEvent>()
        .Should()
        .ContainSingle()
        .Which.Should()
        .Be(new ArticlePublishedEvent(ArticleId: article.Id));
}
```

`result.IsSuccess.Should().BeTrue()` goes. `AdminPublishArticleResult` is
constructed as `new AdminPublishArticleResult(IsSuccess: true)` at
`AdminPublishArticleHandler.cs:52`, so the assertion restates a literal. Drop the
`result` variable with it rather than leaving an unused local.

Where the transition writes more than a status, assert every field it writes.
`ArticleEntity.Reject(string reason)` at `ArticleEntity.cs:475-497` sets `Status`
and `RejectionReason` and raises `CommissionedContentRejectedEvent`, and the
existing test at
`tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/RejectArticle/AdminRejectArticleHandlerTests.cs:56-57`
asserts neither:

```csharp
// after
[Fact]
public async Task Handle_WhenArticleInPendingReview_ShouldRecordStatusAndReason()
{
    // Arrange
    ArticleEntity article = ArticleFactory.CreatePendingReview(CategoryId);
    const string reason = "Sources are not verifiable.";
    var command = new AdminRejectArticleCommand(Id: article.Id.ToString(), Reason: reason);
    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    article.Status.Should().Be(EnumContentStatus.Rejected);
    article.RejectionReason.Should().Be(reason);
    _articleRepositoryMock.VerifyUpdateCalled(article);
}
```

Every handler in the list also has a guard-clause or no-op path, and that path
needs the mirror assertion: the status is unchanged and no event was raised.
`AdminArchiveArticleHandler.cs:35-40` is the clearest, because it branches on the
`bool` that `ArticleEntity.Archive()` (`ArticleEntity.cs:510-527`) returns:

```csharp
// after
[Fact]
public async Task Handle_WhenArticleAlreadyArchived_ShouldNotChangeStateOrRaiseEvents()
{
    // Arrange
    ArticleEntity article = ArticleFactory.CreateArchived(CategoryId);
    article.ClearDomainEvents();
    var command = new AdminArchiveArticleCommand(Id: article.Id.ToString());
    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    // Act
    Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<ConflictException>();
    article.Status.Should().Be(EnumContentStatus.Archived);
    article.DomainEvents.Should().BeEmpty();
    _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

Two in-repo references govern the shape of all of this and should be read before
starting. `ContentOrderEntityTests` at
`tests/Unit/Modules/Content/Domain/Entities/ContentOrderEntityTests.cs:107-147`
splits the transition from the event, asserts the event payload by value with
`ContainSingle().Which.Should().Be(...)`, and asserts `DomainEvents.Should().BeEmpty()`
on the failure path. Identity's
`tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/ActivateRole/AdminActivateRoleHandlerTests.cs:80`
asserts `inactiveRole.IsActive.Should().BeTrue()` on the entity the test supplied,
which is the handler-level equivalent. Copy those two, do not invent a third
convention.

*If done wrong:* asserting the state through the mock instead of on the local
variable reintroduces the indirection the change exists to remove. The entity the
test constructed is in scope on the line after the call; read it directly.

**Acceptance for this change:** deleting the transition call from each handler makes
its test fail. Verify that by hand on `AdminPublishArticleHandler` before declaring
the change done.

### 2. Declare the base type so `BeOfType<T>` carries information

**Files:** the ~26 error-factory and exception test files holding the 112
compiler-guaranteed sites.

```csharp
// tests/Unit/Modules/Core/Application/Shared/Errors/CoreErrorsTests.cs:23-25 — before
BadRequestException exception = _errors.SomeError();

exception.Should().BeOfType<BadRequestException>();
```

```csharp
// after
BaseException exception = _errors.SomeError();

exception.Should().BeOfType<BadRequestException>();
```

The edit is one word per site: the declared type moves to the common base, and the
runtime check becomes the thing being asserted. Where the factory's declared return
type is already the base, no edit is needed — the site is one of the 13 that are
already genuine.

*If done wrong:* changing `BeOfType<T>` to `BeAssignableTo<T>` instead of changing
the variable's declared type weakens the assertion rather than strengthening it.
The exception type maps to an HTTP status, so the exact type is the contract.

### 3. Replace `NotBeNull` on non-nullables with an assertion a mistake can break

**Files:** `tests/Unit/Modules/Content/Infrastructure/Persistence/ContentDbContextTests.cs`,
`tests/Unit/Modules/Identity/Infrastructure/Persistence/IdentityDbContextTests.cs`,
`tests/Unit/Modules/Mailer/Infrastructure/Persistence/MailerDbContextTests.cs`, and
the 23 specification test files under
`tests/Unit/Modules/*/Application/*/Specifications/`.

The three `DbContext` files hold 66 facts and 88 `NotBeNull` assertions between
them, each spinning up a fresh in-memory provider to assert that EF Core
initialised a `DbSet` property:

```csharp
// tests/Unit/Modules/Content/Infrastructure/Persistence/ContentDbContextTests.cs:22-28 — before
[Fact]
public void ContentTypes_ShouldReturnDbSet()
{
    using var context = new ContentDbContext(CreateOptions());
    DbSet<ContentTypeEntity> result = context.ContentTypes;
    result.Should().NotBeNull();
}
```

Collapse the whole `DbSet Properties` region in each file into one theory over the
mapped entity types. `NotBeNull` is legitimate here, because `FindEntityType`
genuinely returns `null` for a type the model does not map, and that is the defect
being hunted:

```csharp
// after
/// <summary>
/// Every domain entity the module owns. A type added here but never configured
/// fails <see cref="Model_ShouldMapEntityWithPrimaryKey" />, which is the failure
/// the per-DbSet tests could not produce.
/// </summary>
public static TheoryData<Type> MappedEntities() =>
    new(
        typeof(ContentTypeEntity),
        typeof(PricingTierEntity),
        typeof(PromotionLevelEntity),
        typeof(ArticleEntity),
        typeof(VideoEntity),
        typeof(LyricsEntity)
    );

[Theory]
[MemberData(nameof(MappedEntities))]
public void Model_ShouldMapEntityWithPrimaryKey(Type entityType)
{
    using var context = new ContentDbContext(CreateOptions());

    IEntityType? mapped = context.Model.FindEntityType(entityType);

    mapped.Should().NotBeNull();
    mapped!.FindPrimaryKey().Should().NotBeNull();
}
```

Enumerate `MappedEntities()` from the existing `DbSet` properties in the file being
replaced, so no entity is dropped in the collapse.

The specification tests are the more misleading variant, because a file exists per
specification and reports it as covered:

```csharp
// tests/Unit/Modules/Content/Application/Editorial/Specifications/ArticleSpecificationsTests.cs:52-63 — before
Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

predicate.Should().NotBeNull();
```

`Compile()` never returns null, so the specification's predicate semantics have
never been evaluated. Evaluate them with `IsSatisfiedBy`, over both the matching and
the non-matching case:

```csharp
// after
[Theory]
[InlineData("fally-ipupa-portrait", true)]
[InlineData("FALLY-IPUPA-PORTRAIT", true)]
[InlineData("koffi-olomide", false)]
public void ArticleBySlugSpecification_ShouldMatchSlugCaseInsensitively(string slug, bool expected)
{
    ArticleEntity article = ArticleFactory.CreateWithSlug(CategoryId, "fally-ipupa-portrait");

    new ArticleBySlugSpecification(slug).IsSatisfiedBy(article).Should().Be(expected);
}
```

The 16 `predicate.Should().NotBeNull()` sites are the ones to hunt first — grep for
them directly. Every one must end up with at least one true case and one false
case, because a specification that returns `true` unconditionally passes a
single-case test.

*If done wrong:* keeping a `NotBeNull()` that immediately precedes a `!`
dereference is correct and must not be swept up; that form is a null guard, not an
assertion. The catalogue records the distinction in entry 1.

### 4. Replace `BeGreaterThanOrEqualTo(n)` with `Be(n)`

**Files:** 33 sites across 25 integration test files, listed by
`grep -rn "BeGreaterThanOrEqualTo([0-9]" tests/`.

Both integration base classes reset the database before every test.
`BaseRepositoryTest.InitializeAsync` calls `Postgres.ResetAsync()`
(`tests/Integration/Common/Base/BaseRepositoryTest.cs:67-70`) and
`BaseApiTest.InitializeAsync` calls `Db.ResetAsync()` before seeding
(`tests/Integration/Common/Base/BaseApiTest.cs:118-126`). Every count therefore has
an exact answer, which the test's own arrangement fixes:

```csharp
// tests/Integration/Modules/Identity/Infrastructure/Repositories/SessionRepositoryTests.cs — before
var sessions = SessionFactory.CreateMany(user.Id, 5);
seedContext.Sessions.AddRange(sessions);
await seedContext.SaveChangesAsync();

var (result, totalCount) = await repo.GetAllWithPaginationAsync(1, 3);

totalCount.Should().BeGreaterThanOrEqualTo(5);
```

```csharp
// after
var sessions = SessionFactory.CreateMany(user.Id, 5);
seedContext.Sessions.AddRange(sessions);
await seedContext.SaveChangesAsync();

var (result, totalCount) = await repo.GetAllWithPaginationAsync(1, 3);

totalCount.Should().Be(5);
result.Should().HaveCount(3);
```

Where the exact number is not obvious because `BaseApiTest` also seeds well-known
test users, count the seeded rows the query can see and write that number. Do not
retreat to an inequality.

*If done wrong:* if a site cannot be given an exact number, the finding is an
isolation defect, not an assertion problem. Record it and fix the isolation; a
weakened assertion that tolerates leakage also tolerates a broken `WHERE` clause.

### 5. Make the five `NotBeNull`-only query-builder test files evaluate their predicates

**Files:**

- `tests/Unit/Modules/Identity/Application/Session/Builders/SessionQueryBuilderTests.cs`
- `tests/Unit/Modules/Identity/Application/Roles/Builders/RoleQueryBuilderTests.cs`
- `tests/Unit/Modules/Identity/Application/Roles/Builders/PermissionQueryBuilderTests.cs`
- `tests/Unit/Modules/Content/Application/Commerce/Builders/ContentOrderQueryBuilderTests.cs`
- `tests/Unit/Modules/Content/Application/Commerce/Builders/ContentPaymentQueryBuilderTests.cs`

These five contain no `IsSatisfiedBy` and no `ToExpression()` call anywhere. Every
assertion is `specification.Should().NotBeNull()` or `.BeNull()`, so the tests
distinguish "a filter was added" from "no filter was added" and nothing else. The
eight sibling query-builder test files already evaluate predicates and are the model.

The proof that this matters is one line in production code:

```csharp
// src/Modules/Identity/Identity/Application/Session/Builders/SessionQueryBuilder.cs:117-120
private void CombineSpecification(Specification<SessionEntity> spec)
{
    _specification = _specification is null ? spec : _specification.And(other: spec);
}
```

Change `_specification.And(other: spec)` to `_specification = spec` — so that each
filter overwrites the previous one instead of intersecting with it — and all 42
assertions in `SessionQueryBuilderTests` still pass. The composite-filter tests at
lines 234, 251, 323 and 338 of the role and permission files assert
`NotBeNull("multiple filters were added")`, which a single surviving filter
satisfies exactly as well as two combined ones. The API would silently start
returning sessions matching *any* filter instead of *all* of them.

Add an evaluation for every filter method and, critically, one for the composition:

```csharp
// after — the composition case that the overwrite bug fails
[Fact]
public void Build_WithUserIdAndActiveStatus_ShouldMatchOnlyEntitiesSatisfyingBoth()
{
    SessionEntity match = SessionFactory.Create(TestUserId);
    SessionEntity wrongUser = SessionFactory.Create(Guid.NewGuid());
    SessionEntity expired = SessionFactory.CreateExpired(TestUserId);

    Specification<SessionEntity>? specification = new SessionQueryBuilder()
        .WithUserId(TestUserId)
        .WithActiveStatus(true)
        .Build();

    specification.Should().NotBeNull();
    specification!.IsSatisfiedBy(match).Should().BeTrue();
    specification.IsSatisfiedBy(wrongUser).Should().BeFalse();
    specification.IsSatisfiedBy(expired).Should().BeFalse();
}
```

`specification.Should().NotBeNull()` stays here, because it is the null guard before
the `!` dereference, not the assertion.

*If done wrong:* asserting only the positive case leaves the overwrite bug green. Each
composed specification needs one entity that fails each constituent filter.

**Note on what this will surface.** `SessionQueryBuilder.WithStatus` lowercases the
incoming status with `status.ToLower()` (`SessionQueryBuilder.cs:33`) before an
invariant-culture comparison. That is the culture-sensitive filter defect recorded
in the remediation plan and owned by spec 13. Do not fix it here; if a predicate
test written under this change exposes it, cross-reference spec 13 rather than
patching `src/`.

### 6. Give the six assertion-free tests an assertion, or delete them

**Files and lines:**

| File | Line | Test |
| --- | --- | --- |
| `tests/Unit/.../UpdateArticleTags/AdminUpdateArticleTagsHandlerTests.cs` | 237 | `Handle_WhenArticleNotFound_ShouldNotInvalidateCache` |
| `tests/Unit/.../UpdateVideoTags/AdminUpdateVideoTagsHandlerTests.cs` | 234 | `Handle_WhenVideoNotFound_ShouldNotInvalidateCache` |
| `tests/Unit/Shared/Exceptions/Handlers/ExceptionHandlerTests.cs` | 183 | `TryHandleAsync_ShouldCallStrategyRegistry` |
| `tests/Unit/.../UpdateOwnProfile/AdminUpdateProfileAuthFactoryTests.cs` | 265 | `UpdateProfileAsync_WithPhoneUsedByCurrentUser_ShouldNotThrow` |
| `tests/Unit/.../UpdateOwnProfile/PublicUpdateProfileAuthFactoryTests.cs` | 398 | `UpdateProfileAsync_WithPhoneUsedByCurrentUser_ShouldNotThrow` |
| `tests/Unit/Modules/Content/Infrastructure/Repositories/ArtistRepositoryTests.cs` | 477 | `GetPublicDirectoryAsync_WithSearch_ShouldMatchFoldedNames` |

The first two are byte-identical copies of the same defect: the test swallows the
exception in a `try/catch` and stops at a bare `// Assert` comment.

```csharp
// tests/Unit/.../UpdateArticleTags/AdminUpdateArticleTagsHandlerTests.cs:236-258 — after
[Fact]
public async Task Handle_WhenArticleNotFound_ShouldNotInvalidateCache()
{
    // Arrange
    Guid nonExistentId = Guid.NewGuid();
    var command = new AdminUpdateArticleTagsCommand(
        ArticleId: nonExistentId.ToString(),
        TagNames: new List<string>()
    );
    _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

    // Act
    Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<NotFoundException>();
    _cacheInvalidatorMock.Verify(x => x.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Never);
    _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

`ThrowAsync<NotFoundException>()` replaces the `try/catch`, so the test now also
fails if the handler stops throwing. `Times.Never` on the invalidator is the
assertion the name promised.

`ExceptionHandlerTests.cs:183` builds a `ProblemDetails` it never uses and then
records, in a comment, that it asserts nothing. Move the expected value to the
assertion side and rename the test after the outcome:

```csharp
// after
[Fact]
public async Task TryHandleAsync_WithUnmappedException_ShouldWriteInternalServerErrorProblemDetails()
{
    // Arrange
    DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
    Exception exception = new("Test error");

    // Act
    bool handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

    // Assert
    handled.Should().BeTrue();
    context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

    ProblemDetails? problem = await HttpTestHelpers.ReadProblemDetailsAsync(context);
    problem.Should().NotBeNull();
    problem!.Status.Should().Be(StatusCodes.Status500InternalServerError);
    problem.Title.Should().Be("Exception");
}
```

The two `UpdateProfileAsync_WithPhoneUsedByCurrentUser_ShouldNotThrow` tests end
with a bare `await _factory.UpdateProfileAsync(...)` under a
`// Act & Assert (should not throw)` comment. "Does not throw" is real behaviour but
is not the whole outcome: the arrangement makes the phone lookup return the *same*
user, and the point is that the duplicate-phone guard does not fire for the caller's
own number. Assert that the profile was written and the transaction committed:

```csharp
// after — replacing the bare call at AdminUpdateProfileAuthFactoryTests.cs:292-303
// Act
await _factory.UpdateProfileAsync(
    userId,
    sessionId,
    null,
    "Rwanda",
    "RW",
    countryDialCode,
    partialPhoneNumber,
    CancellationToken.None
);

// Assert
user.PhoneNumber.Should().Be(fullPhoneNumber);
_unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
```

`ArtistRepositoryTests.cs:477` is different in kind and is deleted, not fixed:

```csharp
// tests/Unit/Modules/Content/Infrastructure/Repositories/ArtistRepositoryTests.cs:473-477 — before
[Fact(
    Skip = "The folded search uses EF.Functions.Like which is not supported by InMemoryDatabase — tested in integration tests"
)]
public Task GetPublicDirectoryAsync_WithSearch_ShouldMatchFoldedNames() => Task.CompletedTask;
```

The skip reason is correct and the test is unreachable by design. Delete the member
and confirm the named coverage exists in
`tests/Integration/Modules/Content/Infrastructure/Repositories/ArtistRepositoryTests.cs`;
if it does not, that is a coverage gap for spec 12, not a reason to keep an empty
unit test.

*If done wrong:* leaving the `try/catch` in place while adding assertions keeps the
"handler stopped throwing" failure mode invisible. The `try/catch` must go.

## Expected fallout

**Change 1 will turn handler tests red, and every red one is a finding.** Two shapes
to expect. First, a test that arranged an entity in the wrong starting state — the
transition was a no-op and nobody noticed, because `IsSuccess` was hard-coded. Fix
the arrangement. Second, a handler that updates a *different* entity than the one it
looked up; `VerifyUpdateCalled(article)` fails where `It.IsAny<ArticleEntity>()`
passed. That is a production defect and gets its own ticket, not a relaxed
assertion.

**Change 3 removes roughly 60 passing tests from the run report.** The three
`DbContext` files drop from 66 facts to three theories. The count falling is
correct: those tests asserted a framework guarantee, and the theory that replaces
them catches an entity added to the domain but never mapped, which none of the 66
could.

**Change 4 will expose isolation leaks.** A site that fails on `Be(n)` with a count
larger than `n` is a test seeing rows another test left behind, or seeing the
well-known users seeded by `BaseApiTest`. Diagnose which before changing anything —
the first is a spec 02 regression, the second is an arithmetic error in the new
expected value.

**Change 5 may turn the session status filter red under a non-invariant locale.**
That is the `ToLower()` defect and it is spec 13's. Record the failure, do not
change `src/`.

**Change 6 reduces the unit test count by one** (the deleted `ArtistRepositoryTests`
member) and turns five previously-green tests into tests that can now fail.

## Testing

```bash
dotnet test tests/Unit
dotnet test tests/Integration
```

Both suites green. Run the integration suite twice back to back and compare
results; change 4 makes count assertions exact, so a difference between the two runs
is an isolation defect that was previously being absorbed.

Grep-provable invariants after this spec:

```bash
# no parameterless update verification survives
grep -rn "VerifyUpdateCalled()" tests/                        # → nothing

# the query-builder tests evaluate predicates
grep -rLn "IsSatisfiedBy" tests/Unit/**/Builders/*QueryBuilderTests.cs   # → nothing

# no inequality against an arranged count
grep -rn "BeGreaterThanOrEqualTo([0-9]" tests/                # → nothing

# no bare Assert comment
grep -rn -A 1 "// Assert$" tests/ | grep -B 1 "^\s*}"          # → nothing

# Compile() is no longer asserted for non-nullness
grep -rn "predicate.Should().NotBeNull()" tests/               # → nothing
```

The new tests that prove the fix, and the mutation each one catches:

| New test | Mutation it catches |
| --- | --- |
| `Handle_WhenArticleIsApproved_ShouldTransitionToPublished` | deleting `article.Publish();` from the handler |
| `Handle_WhenArticleIsApproved_ShouldRaiseArticlePublishedEvent` | removing the `AddDomainEvent` call from `ArticleEntity.Publish` |
| `Handle_WhenArticleAlreadyArchived_ShouldNotChangeStateOrRaiseEvents` | making `Archive()` return `true` on an already-archived article |
| `Model_ShouldMapEntityWithPrimaryKey` | adding an entity to the domain without a configuration |
| `Build_WithUserIdAndActiveStatus_ShouldMatchOnlyEntitiesSatisfyingBoth` | `CombineSpecification` overwriting instead of `.And()` |
| `ArticleBySlugSpecification_ShouldMatchSlugCaseInsensitively` | making the slug comparison case-sensitive |

Verify at least the first and the fifth by actually applying the mutation locally
and confirming red. A mutation check on two representative cases is cheap and is
the only evidence that the change achieved its purpose.

## Risks

**The 24 handler files are a large mechanical edit and mechanical edits drift.** Do
them module by module, starting with `Content/Editorial` where the shape repeats
most, and review the first three carefully so the pattern is settled before the
remaining 21 are written against it.

**Asserting domain events couples tests to event payloads.** That coupling is
intended — the payload is a contract consumed by event handlers — but a payload
change now breaks two places. Assert the event by value with
`ContainSingle().Which.Should().Be(...)` so the failure message names the field that
changed, rather than asserting field-by-field.

**Exact counts are more brittle than inequalities, by design.** The mitigation is
sequencing: specs 01 and 02 must be in before change 4, and the double-run in the
Testing section catches what remains. If a specific site genuinely cannot be made
exact, leave it, add a code comment naming the shared seed row responsible, and
raise it against spec 02 rather than silently keeping the inequality.

**Collapsing the `DbContext` tests loses per-`DbSet` granularity.** If a `DbSet`
property is renamed, the theory still passes as long as the entity is mapped. That
is acceptable: a `DbSet` rename is a compile error at every call site in `src/`, so
the test was never the mechanism protecting it.

**Change 5 may find that a query builder is genuinely wrong.** Budget for it. A
predicate that has never been evaluated has never been checked, and two of the five
files cover commerce filters that decide which orders an admin sees.

## Implementation notes

Verified 2026-08-24 against the tree, with the greps this spec's Testing section
names.

| Change | Invariant | Measured |
| --- | --- | --- |
| 1 | `grep -rn "VerifyUpdateCalled()" tests/` | 0 |
| 2 | `grep -rn "BeOfType<" tests/Unit` | 83 |
| 3 | `grep -rn "predicate.Should().NotBeNull()" tests/` | 0 |
| 3 | specification test files with no predicate evaluation | 0 of 36 |
| 4 | `grep -rn "BeGreaterThanOrEqualTo([0-9]" tests/` | 0 |
| 4 | `grep -rn "BeGreaterThanOrEqualTo" tests/` | 1 |
| 5 | the five named query-builder files without `IsSatisfiedBy` | 0 of 5 |
| 6 | `grep -rn "Fact(Skip" tests/` | 0 |

**Change 4 landed in full.** All 33 numeric-literal sites were converted across ~26
`tests/Integration` files. The worked example in the change list is now
`SessionRepositoryTests.cs:184`, reading `totalCount.Should().Be(5)` after seeding
exactly five sessions, with `result.Should().HaveCount(3)` on the line below. The one
surviving match, `RateLimitingExtensionTests.cs:176`, asserts
`BeGreaterThanOrEqualTo(TimeSpan.Zero)` on a `Retry-After` duration the suite does not
control; it is the site this change exempts by name. This is what
[14-verification-checklist.md](14-verification-checklist.md) invariant C3 now measures.

**Three sites were tightened rather than merely converted**, because converting them
alone would have left an assertion that still could not fail. Only the first is one of
the 33; the other two are neighbouring weak assertions the sweep picked up while it
was in the file, so they were never in this change's count:

| Site | Before | After |
| --- | --- | --- |
| `tests/Integration/Modules/Content/Infrastructure/Mappers/ArticleMapperTests.cs:121` | `BeGreaterThanOrEqualTo(1)` | `Be(2)`, against a 250-word body seeded at `:94` |
| `tests/Integration/Modules/Content/Infrastructure/Repositories/ArticleRepositoryTests.cs:74` | `HaveCountGreaterThanOrEqualTo(2)` | `HaveCount(2)` |
| `tests/Integration/Modules/Content/Infrastructure/Repositories/LyricsRepositoryTests.cs:37` | `NotBeEmpty()` | `HaveCount(3)` |

The mapper row is the one worth reading. Read time is `Math.Max(1, ceil(words / 200))`,
so `BeGreaterThanOrEqualTo(1)` was unfalsifiable by construction — the floor guarantees
it for any body, including an empty one. Seeding 250 words and asserting `Be(2)` pins
the 200-words-per-minute formula instead of the floor.

**No site was found that could not be given an exact number**, so change 4 surfaced no
isolation defect. That is the outcome the change's *If done wrong* note asked to be
recorded either way.

**Change 3 is met in substance across the whole specification suite, not only the
three `DbContext` files.** All 36 files under a `Specifications/` folder in
`tests/Unit` evaluate their predicate — 32 through `IsSatisfiedBy`, and four
(`ContentOrderSpecificationTests`, `ArtistContentSpecificationsTests`,
`CategorySpecificationTests`, `CatalogSpecificationsTests`) through a local `Matches`
helper that compiles the expression. No specification test asserts compilation alone.

**Change 2 is reduced, not proven.** 83 `BeOfType<T>` assertions remain in
`tests/Unit`. The sites named in the change list were converted, but each survivor
was not re-checked against the declared type of its subject, so the box stays
unticked rather than claiming an audit that was not run.

**Change 5 landed for the five files it names**, and only those. Seven other
`*QueryBuilderTests.cs` files in `tests/Unit` contain no `IsSatisfiedBy` —
`ShortVideoQueryBuilderTests`, `ArticleQueryBuilderTests`,
`PopularVideosQueryBuilderTests`, `PopularArticlesQueryBuilderTests`,
`VideoQueryBuilderTests`, `PopularTagsQueryBuilderTests` and
`AllTagsQueryBuilderTests`. They were outside this spec's scope, so the wildcard
invariant in the Testing section (`grep -rLn "IsSatisfiedBy" tests/Unit/**/Builders/*QueryBuilderTests.cs`
→ nothing) is over-broad as written; the invariant that holds is the one over the
five named files.

## Checklist

- [x] 1 — `VerifyUpdateCalled` takes the expected entity on every repository mock;
      all 24 handler tests assert the destination status, the fields the transition
      writes, and the domain events, with the no-op path asserting
      `DomainEvents.Should().BeEmpty()`
- [ ] 2 — the 112 compiler-guaranteed `BeOfType<T>` sites declare the base type
- [x] 3 — the three `DbContext` test files each expose one
      `Model_ShouldMapEntityWithPrimaryKey` theory; every specification test
      evaluates its predicate with `IsSatisfiedBy` over a true and a false case —
      four files evaluate through a local `Matches` helper instead, which is the same
      property
- [x] 4 — all 33 `BeGreaterThanOrEqualTo(n)` sites assert `Be(n)` — converted across
      ~26 integration files; the only surviving match is the `TimeSpan.Zero` duration
      at `RateLimitingExtensionTests.cs:176`, and three sites were tightened past a
      straight conversion because a converted assertion would still not have failed
- [x] 5 — the five query-builder test files evaluate predicates, including one
      composed-filter test per builder that fails if `CombineSpecification`
      overwrites
- [x] 6 — the six assertion-free tests have assertions or are deleted; no `try/catch`
      remains around an act phase — the three surviving `try/catch` blocks in
      `ResourceNotFoundMiddlewareTests.cs:74,106,166` assert on the caught exception
      and throw when nothing was thrown (`:84`), which is the inspection form, not the
      swallowing form spec 07 removed
