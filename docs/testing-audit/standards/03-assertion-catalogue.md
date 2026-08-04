# Assertion catalogue

A reference for review. Each entry gives the form to reject, the reason it cannot fail, and
the form to write instead. The counts are from this suite as measured during the audit.

## The governing heuristic

Before accepting any assertion, ask:

> **Would this test still pass if the method under test were replaced with `return default;`?**

If yes, the assertion is decoration. The variants of that question that catch the most:

- Would it pass if the resource file were emptied?
- Would it pass if the argument order were swapped?
- Would it pass if the state transition were deleted?
- Would it pass against a different threshold value?

An assertion earns its place by naming a specific change to `src/` that would break it. If
a reviewer cannot name one, neither can the test.

## The catalogue

### 1. `NotBeNull` on something that cannot be null

**Bad**

```csharp
using var context = new ContentDbContext(CreateOptions());
DbSet<ContentTypeEntity> result = context.ContentTypes;

result.Should().NotBeNull();
```

**Why it cannot fail** — EF Core initialises every `DbSet` property during context
construction. The variable is non-nullable and the framework guarantees the value. The same
applies to `spec.ToExpression().Compile()`, which never returns null, and to any
`Should().NotBeNull()` on a non-nullable reference the compiler has already proven.

Roughly 1,172 whole-statement `Should().NotBeNull();` assertions exist in the suite.

**Good** — assert the thing a mistake could actually break:

```csharp
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

`NotBeNull` is legitimate here because `FindEntityType` genuinely returns `null` for an
unmapped type — which is the defect being hunted.

**Exception** — `NotBeNull()` immediately before a `!` dereference is a null-guard, not an
assertion, and is fine:

```csharp
found.Should().NotBeNull();
found!.Slug.Should().Be("fally-ipupa-portrait");
```

### 2. `BeOfType<T>` on a variable already declared `T`

**Bad**

```csharp
BadRequestException exception = _errors.SomeError();

exception.Should().BeOfType<BadRequestException>();
```

**Why it cannot fail** — the compiler has already proven the static type. The runtime check
can only fail if the factory returns a *subtype*, and none of these do. 112 of the suite's
125 `BeOfType<T>()` assertions are compiler-guaranteed.

**Good** — declare the variable as the base type, so the assertion carries information:

```csharp
BaseException exception = _errors.RoleAlreadyActive();

exception.Should().BeOfType<ConflictException>();
exception.Message.Should().Be("The role is already active.");
```

### 3. Asserting a hard-coded result flag

**Bad**

```csharp
// src/.../AdminPublishArticleHandler.cs:48
return new AdminPublishArticleResult(IsSuccess: true);   // unconditional
```

```csharp
result.IsSuccess.Should().BeTrue();
```

**Why it cannot fail** — the handler either throws or returns success. The boolean is a
constant in the source, so the assertion restates a literal. `IsSuccess.Should().BeTrue()`
appears 141 times against 99 result types that hard-code `IsSuccess: true`.

**Good** — assert the effect the handler was called for:

```csharp
await _handler.Handle(command, CancellationToken.None);

article.Status.Should().Be(EnumContentStatus.Published);
article.PublishedAt.Should().NotBeNull();
_unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
```

Where the flag genuinely varies, keep it — and add the case where it is `false`, which is
the half that proves it varies.

### 4. Comparing a message against the system's own localizer

**Bad**

```csharp
var i18n = TestErrorsFactory.CreateIdentityI18n();
var validator = new AdminLoginValidator(i18n);
// ...
result.ShouldHaveValidationErrorFor(x => x.Email)
      .WithErrorMessage(i18n.User.Validation.EmailRequired());
```

**Why it cannot fail** — both sides resolve through the same `IStringLocalizer` under the
same culture. Expected equals actual by construction. Empty the French `.resx` and all 208
executions across 104 files still pass, because both sides fall back to the neutral string
together.

**Good** — pin the expected string as a literal, and scope the culture around the *read*:

```csharp
[Theory]
[InlineData("en", "Email is required.")]
[InlineData("fr", "L'adresse e-mail est requise.")]
public async Task Validate_WithMissingEmail_ShouldReturnMessageInRequestCulture(
    string culture,
    string expected
)
{
    var validator = new AdminLoginValidator(TestErrorsFactory.CreateIdentityI18n());

    using var _ = new CultureScope(culture);

    TestValidationResult<AdminLoginCommand> result = await validator.TestValidateAsync(
        new AdminLoginCommand(Email: null!, Password: TestConstants.User.ValidPassword)
    );

    result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(expected);
}
```

Better still, replace the 104 files with one resource-completeness theory that asserts
every key in the neutral `.resx` exists in `en` and `fr` with a non-empty, distinct value.

The generalisation: **never derive the expected value from the system under test.** It
covers localizers, mappers asserted by calling the mapper, and formulas recomputed with the
production expression.

### 5. `BeGreaterThanOrEqualTo(N)` where the arrangement fixes N

**Bad**

```csharp
// tests/Integration/Modules/Identity/Infrastructure/Repositories/SessionRepositoryTests.cs:176-184
var sessions = SessionFactory.CreateMany(user.Id, 5);
seedContext.Sessions.AddRange(sessions);
await seedContext.SaveChangesAsync();

var (result, totalCount) = await repo.GetAllWithPaginationAsync(1, 3);

totalCount.Should().BeGreaterThanOrEqualTo(5);
```

**Why it cannot fail usefully** — the database was reset in `InitializeAsync` and the test
seeded exactly five rows. A repository that returns 5, 6, 500 or every row in the table
passes identically. The inequality was written to tolerate leakage from other tests, and it
succeeds in tolerating a broken `WHERE` clause too.

33 numeric-literal `BeGreaterThanOrEqualTo(n)` assertions exist, almost all against
arrangements that fix `n`.

**Good**

```csharp
totalCount.Should().Be(5);
result.Should().HaveCount(3);
```

If exact counts are genuinely impossible because of shared seed data, the isolation problem
is the finding — fix that rather than weakening the assertion. See
[02-integration-testing-standard.md](02-integration-testing-standard.md).

### 6. `Should().Throw<Exception>()` instead of the specific type

**Bad**

```csharp
// tests/Unit/Modules/Identity/Domain/Entities/UserEntityTests.cs:65
Action act = () => UserEntity.Create(id, invalidEmail!, /* ... */);

act.Should().Throw<Exception>();
```

```csharp
// tests/Integration/Modules/Content/Infrastructure/Repositories/AlbumRepositoryTests.cs:45-47
var act = async () => await repo.GetByIdOrThrowAsync(Guid.NewGuid());

await act.Should().ThrowAsync<Exception>();
```

**Why it cannot fail** — every exception derives from `Exception`. A `NullReferenceException`
from a typo in the arrangement satisfies it just as well as the validation error the test
is named for. 15 such assertions exist across five files.

**Good** — the type, and the part of the message or code the caller depends on:

```csharp
act.Should()
    .Throw<ValidationException>()
    .WithMessage("*email*");
```

```csharp
await act.Should().ThrowAsync<NotFoundException>();
```

For a guard that maps to a specific HTTP status, assert the concrete exception type,
because the status is derived from it.

### 7. `Verify` with `It.IsAny<>` in every position

**Bad**

```csharp
// tests/Unit/Shared/Exceptions/Handlers/ExceptionHandlerTests.cs:219-228
_loggerMock.Verify(
    x =>
        x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ),
    Times.Once
);
```

**Why it cannot fail meaningfully** — it asserts that the handler logged *something*, at any
level, with any message. The suite contains 3,072 `It.IsAny<>` usages and 536 `.Verify(`
calls with no explicit `Times` at all, which default to `Times.AtLeastOnce()`.

**Good** — match the positions the behaviour depends on:

```csharp
_loggerMock.Verify(
    x =>
        x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ),
    Times.Once
);
```

`CancellationToken` and `EventId` stay `It.IsAny<>`. Identifiers never do — an
`It.IsAny<Guid>()` in a lookup means a handler asking for the wrong entity still satisfies
the test. See
[05-mock-defaults-and-dead-helpers.md](../fixtures/05-mock-defaults-and-dead-helpers.md).

### 8. Asserting log output as the outcome

**Bad**

```csharp
_loggerMock.Verify(
    x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) =>
        v.ToString()!.Contains("Article published")), /* ... */),
    Times.Once
);
```

**Why it is the wrong assertion** — log text is not a contract. Reword the message and the
test fails without any behaviour changing; change the behaviour while keeping the message
and the test passes. It also couples the test to `ToString()` on a compiler-generated
`FormattedLogValues` struct.

**Good** — assert the behaviour, and verify the log line only where logging *is* the
feature (audit trails, security events). In that case assert the level and the structured
property, never the rendered sentence.

### 9. Asserting a value the mock was told to return

**Bad**

```csharp
_repository.SetupGetByIdAsync(article.Id, article);

AdminGetArticleResult result = await _handler.Handle(query, CancellationToken.None);

result.Title.Should().Be(article.Title);
```

**Why it barely fails** — if the handler passes the entity through untouched, this asserts
that the mock returned what the mock was told to return. It proves nothing about mapping,
filtering or projection. It fails only if the handler drops the property entirely.

**Good** — assert something the handler *computes*, and let a dedicated mapper test cover
the passthrough:

```csharp
result.Slug.Should().Be(article.Slug);
result.ReadTimeInMinutes.Should().Be(4);        // derived from body length by the mapper
result.IsBookmarked.Should().BeFalse();          // derived from the caller's identity
```

Passthrough fields are worth one assertion in the mapper's own test, not one per handler
test.

### 10. `BeOneOf` on a status code

**Bad**

```csharp
// tests/Integration/Modules/Identity/Application/User/UseCases/Public/Commands/UpdateOwnProfile/V1/PublicUpdateOwnProfileEndpointV1Tests.cs:139
response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
```

```csharp
// tests/Integration/Modules/Content/Application/Editorial/UseCases/Admin/Commands/UploadArticleImage/V1/AdminUploadArticleImageEndpointV1Tests.cs:80
.BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
```

**Why it cannot fail** — one request produces one status. `BeOneOf` records that the author
did not know which, and freezes the ambiguity into the suite. A client integrating against
this API has to know: `403` and `400` mean different things and are handled differently.
Eight such sites exist.

**Good** — decide the contract, assert it, and fix `src/` if the answer is wrong:

```csharp
await response.ShouldBeProblem<RefreshTokenExpiryException>(
    HttpStatusCode.Forbidden,
    Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
);
```

`BeOneOf` is legitimate only where the value is genuinely non-deterministic by design, which
a status code never is.

### 11. An error assertion that pins only the status

**Bad**

```csharp
await response.ShouldBeProblem(HttpStatusCode.NotFound);
```

**Why it barely fails** — it proves the body was RFC 7807 and the number was 404. Every
non-trivial handler reaches 404 from several guards, and the test name claims one of them.
A test named `..._NonExistentLike_ReturnsNotFound` passed for years against a 404 raised by
the article lookup two lines earlier, while the like guard it named was a
`BadRequestException` that no test ever reached.

**Good** — status, the exception type behind it, and the exact localized detail:

```csharp
await response.ShouldBeProblem<NotFoundException>(
    HttpStatusCode.NotFound,
    Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
);
```

`Title` is `nameof(TException)` in every exception strategy, so the type argument is a
compile-checked expectation; it separates two exception *types* that share a status (four
produce 403) but not two guards inside one handler. `Detail` is what separates guards.

Two rules make the detail assertion honest:

- **Resolve it, never type it.** `Localized<TMessage>` invokes the application's own
  message class from the host container, so a `.resx` reword moves both sides together.
  This is not entry 4's self-comparison — there both sides came from the same localizer
  call, here the actual value arrives over HTTP.
- **Resolve it in the culture the request selects.** The default is `fr`, not `en`
  (`LocalizationExtension.cs:22`), and a test that sends no `Accept-Language` header gets
  French. Pass `LocalizedMessage.EnglishCulture` only where the test sets the header.

A third `ShouldBeProblem(status, string)` overload exists, is `[Obsolete]`, and
substring-matches the detail. It is a migration shim; reject it in review.

## Quick reference

| # | Reject | Because | Write instead |
| --- | --- | --- | --- |
| 1 | `NotBeNull()` on a non-nullable | compiler/framework guarantees it | assert what a mistake breaks |
| 2 | `BeOfType<T>()` on a `T` variable | compiler already proved it | declare the base type |
| 3 | `IsSuccess.Should().BeTrue()` on a constant | restates a literal | assert the state change |
| 4 | expected value from the system's own localizer | compares a value to itself | literal expected, `CultureScope` around the read |
| 5 | `BeGreaterThanOrEqualTo(n)` with `n` arranged | passes for any larger result | `Be(n)` |
| 6 | `Throw<Exception>()` | every failure satisfies it | the concrete type, plus message or code |
| 7 | `Verify` all-`It.IsAny<>` | proves a call, not a correct call | match meaningful positions, explicit `Times` |
| 8 | asserting rendered log text | log text is not a contract | assert behaviour; level + structured property if logging is the feature |
| 9 | asserting a value the mock returned | asserts the mock | assert a computed field |
| 10 | `BeOneOf` on a status code | one request, one status | decide the contract |
| 11 | `ShouldBeProblem(status)` alone | several guards reach one status | `ShouldBeProblem<TException>(status, Localized<TMessage>(...))` |
