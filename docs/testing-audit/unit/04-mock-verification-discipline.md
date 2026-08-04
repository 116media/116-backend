# High — Mock verification proves calls, not outcomes

The unit suite verifies interactions well and outcomes poorly. 3,027 lines use
`It.IsAny<>` against 141 uses of `It.Is<>`, 658 tests (9.8% of 6,686) assert nothing
except that a mock was called, 108 verifications match two or more real arguments
without checking a single one, and 6 tests assert nothing at all. The result is a
suite that can prove a method was invoked and, in a large minority of cases, cannot
prove anything about what it was invoked with or what it produced.

## The problem

### Six tests assert nothing

Two of them are byte-identical copies of the same defect. The test declares an
intent in its name, executes the handler, writes `// Assert`, and stops:

```csharp
// tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/UpdateArticleTags/AdminUpdateArticleTagsHandlerTests.cs:236-257
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
    try
    {
        await _handler.Handle(command, CancellationToken.None);
    }
    catch (NotFoundException)
    {
        // Expected
    }

    // Assert
}
```

The identical body appears at
`tests/Unit/.../UpdateVideoTags/AdminUpdateVideoTagsHandlerTests.cs:234`. The
assertion the name promises — `_cacheInvalidatorMock.Verify(x => x.InvalidateAsync(...), Times.Never)`
— is one line and is not there. Worse, the `try/catch` suppresses the exception,
so the test also cannot fail if the handler stops throwing.

The remaining four are:

| File | Line | Test |
| --- | --- | --- |
| `tests/Unit/Shared/Exceptions/Handlers/ExceptionHandlerTests.cs` | 183 | `TryHandleAsync_ShouldCallStrategyRegistry` |
| `tests/Unit/.../UpdateOwnProfile/AdminUpdateProfileAuthFactoryTests.cs` | 265 | `UpdateProfileAsync_WithPhoneUsedByCurrentUser_ShouldNotThrow` |
| `tests/Unit/.../UpdateOwnProfile/PublicUpdateProfileAuthFactoryTests.cs` | 398 | `UpdateProfileAsync_WithPhoneUsedByCurrentUser_ShouldNotThrow` |
| `tests/Unit/Modules/Content/Infrastructure/Repositories/ArtistRepositoryTests.cs` | 477 | `GetPublicDirectoryAsync_WithSearch_ShouldMatchFoldedNames` |

The exception-handler case is the most instructive, because it builds a
`ProblemDetails` it never uses and then documents, in a comment, why it asserts
nothing:

```csharp
// tests/Unit/Shared/Exceptions/Handlers/ExceptionHandlerTests.cs:182-200
[Fact]
public async Task TryHandleAsync_ShouldCallStrategyRegistry()
{
    // Arrange
    DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
    Exception exception = new("Test error");
    ProblemDetails problemDetails = new()
    {
        Status = StatusCodes.Status500InternalServerError,
        Title = "Exception",
        Detail = "Test error",
    };

    // Act
    await _handler.TryHandleAsync(context, exception, CancellationToken.None);

    // Assert
    // Registry is called internally, verification not needed with real implementation
}
```

The comment is the honest half of the analysis and the wrong conclusion. If the
registry cannot be verified because it is a real implementation, the observable
outcome — the status code and body written to `context.Response` — can be, and is
what the test's name is really about.

### 658 tests assert only that a mock was called

Nearly one test in ten in the unit suite contains one or more `Verify` calls and no
`Should()` or `Assert.` at all. A mock verification answers "was this collaborator
invoked?"; it does not answer "did the method do its job?". For a handler that
returns a mapped response, filters a list, or computes a total, the collaborator
call is the least interesting thing that happened.

### 108 verifications check a call shape and no arguments

Of the 186 `Verify` calls in the unit suite that match two or more arguments
(excluding `CancellationToken`, which is legitimately `It.IsAny`), 108 use
`It.IsAny<>` for every one of them. A representative case:

```csharp
// tests/Unit/.../UpdateArticleTags/AdminUpdateArticleTagsHandlerTests.cs:97-124
[Fact]
public async Task Handle_WhenTagNamesAreNew_ShouldCreateTagsAndReturnSuccess()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId);

    var command = new AdminUpdateArticleTagsCommand(
        ArticleId: article.Id.ToString(),
        TagNames: new List<string> { "Afrobeats", "Rumba" }
    );
    ...
    _lookupRepositoryMock.Verify(
        x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
        Times.Exactly(2)
    );
}
```

The test's name says the handler creates the *new* tags. The verification says two
tags were created. It would pass if the handler created "Afrobeats" twice, or
created two tags named after the article's slug, or transposed the two names. The
one thing the test exists to check — that the tag names from the command reach the
repository — is the thing `It.IsAny<TagEntity>()` declines to look at.

### What is already right, and should be protected

Three findings run the other way and are worth stating plainly, because they
constrain the remediation.

**`Times` is essentially universal.** Of 951 Moq `Verify` calls in the unit suite,
950 pass an explicit `Times` and exactly one does not
(`tests/Unit/Modules/Content/Application/Editorial/EventHandlers/VideoYoutubeUrlAttachedThumbnailHandlerTests.cs:155`).
A line-oriented grep suggests 529 `Verify` calls lack `Times`, but that is a
measurement artifact of multi-line formatting — the argument is on a later line.
There is no `Times` discipline problem in this suite.

**`MockBehavior.Strict` is absent everywhere.** `MockBehavior` appears zero times
in `tests/`, so every mock is loose. Combined with reference-typed returns
defaulting to `null`, an unstubbed call usually surfaces as a `NullReferenceException`
inside the handler rather than as a silently satisfied dependency. That is a blunt
safety net, but it is a real one, and it is why the missing-argument problem has not
produced more false greens than it has.

**The interceptor tests are the counter-example to copy.** The reviewer's note that
`DispatchDomainEventsInterceptorTests.cs:120` asserts a count without identity does
not hold on inspection; the count assertion is immediately followed by three
identity assertions:

```csharp
// tests/Unit/Shared/Infrastructure/Interceptors/DispatchDomainEventsInterceptorTests.cs:120-123
publisherMock.Verify(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
publisherMock.Verify(p => p.Publish(event1, It.IsAny<CancellationToken>()), Times.Once);
publisherMock.Verify(p => p.Publish(event2, It.IsAny<CancellationToken>()), Times.Once);
publisherMock.Verify(p => p.Publish(event3, It.IsAny<CancellationToken>()), Times.Once);
```

Publishing the same event three times fails the second, third and fourth lines.
This is the correct pattern — a count assertion for the cardinality, plus one
identity assertion per expected argument — and it is already in the codebase.

## Why it matters

A verification-only test locks in the handler's call graph while leaving its
behaviour free. That is the opposite of what a test should do: it makes refactoring
expensive (any change to which collaborator is called breaks tests) while making
regressions cheap (any change to what is passed goes unnoticed).

The concrete failure for the tag case: a developer reorders the loop that maps
command tag names to `TagEntity` instances and introduces an off-by-one that
creates the first tag twice. Two `AddTagAsync` calls happen. `Times.Exactly(2)`
passes. The article ends up with a duplicate tag and no second tag, and the defect
reaches the integration suite only if an integration test happens to assert the
persisted tag names.

The assertion-free tests are a distinct and larger problem, because they are
counted. `Handle_WhenArticleNotFound_ShouldNotInvalidateCache` appears in the run
report as a passing test named after a behaviour that is not tested. Anyone reading
the file to decide whether cache invalidation is covered will conclude that it is.
That is the mechanism by which a test is worse than no test.

## The fix

### Assert the argument that matters

```csharp
// Before
_lookupRepositoryMock.Verify(
    x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
    Times.Exactly(2)
);
```

```csharp
// After
_lookupRepositoryMock.Verify(
    x => x.AddTagAsync(It.Is<TagEntity>(t => t.Name == "Afrobeats"), It.IsAny<CancellationToken>()),
    Times.Once
);
_lookupRepositoryMock.Verify(
    x => x.AddTagAsync(It.Is<TagEntity>(t => t.Name == "Rumba"), It.IsAny<CancellationToken>()),
    Times.Once
);
_lookupRepositoryMock.Verify(
    x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
    Times.Exactly(2)
);
```

`CancellationToken` stays `It.IsAny` — it is plumbing, not behaviour. Every other
argument is either named or predicated. The trailing cardinality check ensures no
third tag was created.

Where the predicate would be long, capture the arguments instead and assert on them
with the full expressiveness of the assertion library:

```csharp
var created = new List<TagEntity>();
_lookupRepositoryMock
    .Setup(x => x.AddTagAsync(Capture.In(created), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);

await _handler.Handle(command, CancellationToken.None);

created.Select(t => t.Name).Should().Equal("Afrobeats", "Rumba");
```

### Give the assertion-free tests their assertion

```csharp
// Before — tests/Unit/.../UpdateArticleTags/AdminUpdateArticleTagsHandlerTests.cs:236-257
[Fact]
public async Task Handle_WhenArticleNotFound_ShouldNotInvalidateCache()
{
    ...
    try
    {
        await _handler.Handle(command, CancellationToken.None);
    }
    catch (NotFoundException)
    {
        // Expected
    }

    // Assert
}
```

```csharp
// After
[Fact]
public async Task Handle_WhenArticleNotFound_ShouldNotInvalidateCache()
{
    Guid nonExistentId = Guid.NewGuid();
    var command = new AdminUpdateArticleTagsCommand(
        ArticleId: nonExistentId.ToString(),
        TagNames: new List<string>()
    );
    _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

    Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
    _cacheInvalidatorMock.Verify(x => x.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Never);
    _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

`Should().ThrowAsync<T>()` replaces the `try/catch`, so the test also fails if the
handler stops throwing. `Times.Never` on the invalidator is the assertion the name
promised, and `Times.Never` on the commit adds the related guarantee that nothing
was persisted.

### Assert the outcome, not only the registry call

```csharp
// Before — tests/Unit/Shared/Exceptions/Handlers/ExceptionHandlerTests.cs:182-200
// Assert
// Registry is called internally, verification not needed with real implementation
```

```csharp
// After
[Fact]
public async Task TryHandleAsync_WithUnmappedException_ShouldWriteInternalServerErrorProblemDetails()
{
    DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
    Exception exception = new("Test error");

    bool handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

    handled.Should().BeTrue();
    context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

    ProblemDetails? problem = await HttpTestHelpers.ReadProblemDetailsAsync(context);
    problem.Should().NotBeNull();
    problem!.Status.Should().Be(StatusCodes.Status500InternalServerError);
    problem.Title.Should().Be("Exception");
}
```

The unused `ProblemDetails` in the original was the author writing down the expected
outcome and then not asserting it. Moving it to the assertion side turns a
zero-information test into a contract test for the error response shape.

## The principle

**A mock verification is evidence about the collaborator, never about the subject.**
It belongs in a test as a supporting assertion, alongside an assertion about the
value the method returned or the state it changed — not instead of one.

Three rules follow:

1. **Every argument that carries meaning is matched by value or predicate.**
   `It.IsAny<T>()` is correct for `CancellationToken` and for arguments the test
   genuinely does not care about. If the test's name refers to an argument, that
   argument may not be `It.IsAny`.
2. **Every `Verify` carries an explicit `Times`.** This suite already does it 950
   times out of 951; keep it at 951.
3. **A test with only `Verify` calls is unfinished.** Ask what the caller observes —
   a return value, a mutated entity, a status code, a thrown exception — and assert
   that too. A test body containing a bare `// Assert` comment is a defect, not a
   style issue.

## Checklist

- [ ] The test body contains at least one assertion about a return value, entity
      state, or thrown exception — not only `Verify` calls.
- [ ] No `// Assert` comment is followed by nothing.
- [ ] `try/catch` around the act phase is replaced by `Should().ThrowAsync<T>()`.
- [ ] Arguments named in the test's method name are matched with `It.Is<T>(...)` or
      by value, never `It.IsAny<T>()`.
- [ ] Cardinality assertions (`Times.Exactly(n)`) are accompanied by one identity
      assertion per expected argument.
- [ ] Every `Verify` passes an explicit `Times`.
- [ ] "Should not happen" tests assert `Times.Never` on the thing that should not
      happen.
