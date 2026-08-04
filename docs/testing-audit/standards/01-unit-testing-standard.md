# Unit testing standard

What a test under `tests/Unit/` must do to be worth its place in the suite. This is the
target state, written so a reviewer can apply it line by line in a pull request. The
evidence for each rule — what the suite does today and what it costs — is in the findings
documents linked from each section.

The project runs **xUnit v3 1.1.0**, **AwesomeAssertions 9.0.0**, **Moq 4.20.72** and
**Bogus 35.6.3**, targeting **.NET 9**. Everything below is written against those.

## Scope

`tests/Unit/` owns, exhaustively, the things integration tests deliberately skip:

- domain entity guards and **every** state transition, including no-op and early-return
  branches
- validator rules and their boundaries
- handler orchestration against mocked owned abstractions
- error factory methods
- specification predicate logic
- mappers and pure functions

It never touches a real database, a real HTTP pipeline, or a DI container. A unit test
that needs any of those is an integration test in the wrong folder; see
[02-integration-testing-standard.md](02-integration-testing-standard.md).

## 1. One behaviour per test

A test asserts one thing about one call. When a test needs `and` to describe what it
checks, it is two tests.

This is not a style preference. A test with four unrelated assertions stops at the first
failure, so the other three findings are hidden until someone fixes the first — and the
name at the top of the CI report describes only a quarter of what broke.

Multiple assertions about the **same** outcome are one behaviour and belong together:

```csharp
[Fact]
public async Task Handle_WithValidCommand_ShouldPublishTheArticle()
{
    ArticleEntity article = ArticleFactory.CreateApproved(_categoryId);
    _repository.SetupGetByIdOrThrow(article);

    await _handler.Handle(new AdminPublishArticleCommand(article.Id), CancellationToken.None);

    article.Status.Should().Be(EnumContentStatus.Published);
    article.PublishedAt.Should().NotBeNull();
}
```

`Status` and `PublishedAt` are two facets of one transition. Adding "and the slug is
unchanged" to the same test is a second behaviour.

## 2. Arrange, Act, Assert — visibly

Three blocks, separated by blank lines, in that order. One statement in the Act block.

If the Act block has two calls, the test is asserting a sequence, and the reader cannot
tell which call the assertion is about. If the Arrange block runs after the Act block, the
test is asserting against state it set up afterwards.

The `// Arrange` / `// Act` / `// Assert` comments are optional. The blank lines are not:
they are what makes the shape scannable in a diff.

## 3. Naming: `Method_Scenario_ExpectedResult`

The suite already does this well and it should be protected. Of 8,568 test methods, every
single one uses the underscore-delimited form; 6,822 use the full three-part shape and
1,746 use the two-part `Method_ExpectedResult` shape for cases with no meaningful scenario.

```csharp
// good — all three parts present
public async Task Handle_WhenArticleIsAlreadyPublished_ShouldThrowConflictException()
public void HasMaxAttemptsReached_AtTheThreshold_ShouldReturnTrue()

// acceptable — no scenario worth naming
public void Constructor_ShouldInitialiseAnEmptyDomainEventCollection()

// not acceptable — describes the mechanism, not the expectation
public async Task Handle_ShouldWork()
public async Task Handle_CallsRepository()
```

`ExpectedResult` states an outcome the reader could disagree with. "Works" is not an
outcome, and "calls the repository" is a mechanism — see rule 4.

## 4. Assert outcomes, not interactions

A test asserts what the caller observes: the returned value, the mutated aggregate, the
thrown exception. It reaches for `Verify` only when the interaction *is* the outcome.

```csharp
// bad — proves a method was reached, not that anything happened
await _handler.Handle(command, CancellationToken.None);

_repository.Verify(x => x.Update(It.IsAny<ArticleEntity>()), Times.Once);
result.IsSuccess.Should().BeTrue();

// good — proves the transition, which is what the handler exists to perform
await _handler.Handle(command, CancellationToken.None);

article.Status.Should().Be(EnumContentStatus.Published);
_unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
```

The `CommitAsync` verification survives because persistence genuinely is an outcome a unit
test cannot observe any other way. `Update(It.IsAny<...>())` does not: the entity is in
hand, so assert on the entity.

`IsSuccess.Should().BeTrue()` against a result type that hard-codes `IsSuccess: true` is
never an assertion. See [unit/01](../unit/01-assertions-that-cannot-fail.md) and
[unit/04](../unit/04-mock-verification-discipline.md).

## 5. Never derive the expected value from the system under test

The expected value is a literal, or comes from a source independent of the code being
tested. The moment a test asks the same object the production code asks, it is comparing a
value to itself and will pass for every possible answer.

```csharp
// bad — both sides resolve through the same localizer, in the same culture
var i18n = TestErrorsFactory.CreateIdentityI18n();
var validator = new AdminLoginValidator(i18n);

result.ShouldHaveValidationErrorFor(x => x.Email)
      .WithErrorMessage(i18n.User.Validation.EmailRequired());

// good — the resource file is actually consulted
using var _ = new CultureScope("fr");

result.ShouldHaveValidationErrorFor(x => x.Email)
      .WithErrorMessage("L'adresse e-mail est requise.");
```

The same rule bars recomputing an expected value with the production formula, asserting a
mapper's output by calling the mapper, and asserting a constant by referencing the constant
the code under test reads. Production *constants* are the one exception and are in fact
required — see [04-test-data-and-fixtures.md](04-test-data-and-fixtures.md); the
distinction is that a constant is an input to both sides, not an answer computed by one of
them.

## 6. `[Theory]` + `[MemberData]` is the default for any repeated shape

Three tests that differ only in a literal are one theory. The suite currently has 8,272
`[Fact]` methods to 298 `[Theory]` methods and **4** `[MemberData]` usages, which is a
copy-paste ratio rather than a style choice; the cost is documented in
[unit/05](../unit/05-duplication-and-theories.md).

Use `[InlineData]` for scalar cases and `[MemberData]` with a `TheoryData<...>` property
for anything typed:

```csharp
public static TheoryData<string, bool> SlugCases() =>
    new()
    {
        { "fally-ipupa-portrait", true },
        { "FALLY-IPUPA-PORTRAIT", true },
        { "koffi-olomide", false },
    };

[Theory]
[MemberData(nameof(SlugCases))]
public void IsSatisfiedBy_ShouldMatchSlugCaseInsensitively(string slug, bool expected)
{
    ArticleEntity article = ArticleFactory.CreateWithSlug(_categoryId, "fally-ipupa-portrait");

    new ArticleBySlugSpecification(slug).IsSatisfiedBy(article).Should().Be(expected);
}
```

`TheoryData<T>` over raw `object[]` — it is type-checked at compile time, so a case with
the wrong arity fails the build rather than the run.

The reason this matters is not concision. A duplicated block is never audited for the case
it forgot; a `TheoryData` collection is a visible list of the cases considered, and the
missing row is apparent.

## 7. What to mock, and what never to mock

**Mock only abstractions this codebase owns that perform real I/O.** In practice:

| Mock | Do not mock |
| --- | --- |
| `IArticleRepository`, `ISessionRepository`, … | domain entities and aggregates |
| `IUnitOfWork` | value objects (`Email`, `AuthProvider`, `OtpPurpose`) |
| `IPasswordService`, `IOtpService` | pure functions and extension methods |
| `IEmailSender`, `ICloudinaryService` | mappers and specifications |
| `IFileRepository` | error factories and `*I18n` facades |
| | `DbContext` |

The right-hand column is not a matter of taste. A mocked entity asserts against a state the
domain may not be able to reach, which is exactly the defect
[01-constant-drift.md](../fixtures/01-constant-drift.md) documents. A mocked mapper turns a
mapping test into a test of the mock. A mocked error factory removes the only thing the
handler's error path was going to prove.

Use the real `TestErrorsFactory` facades, the real value objects and the real builders. If
constructing the real thing is painful, that is information about the production design,
not a reason for a mock.

## 8. `Times` on every `Verify`, `It.Is<T>` for arguments that matter

`mock.Verify(...)` without a `Times` argument defaults to `Times.AtLeastOnce()`, so it
cannot distinguish one call from fifty. 536 of the suite's 1,015 `.Verify(` calls omit it.

```csharp
// bad — passes if the handler sends four emails
_mailer.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()));

// good — the count is part of the contract
_mailer.Verify(
    x => x.SendAsync(It.Is<EmailMessage>(m => m.To == user.Email), It.IsAny<CancellationToken>()),
    Times.Once
);
```

Argument matching is decided **per position**:

- `It.IsAny<CancellationToken>()` — correct. No assertion should depend on token identity.
- `It.IsAny<Guid>()` in an identifier position — never correct. The identifier is usually
  the thing the test is checking; see
  [05-mock-defaults-and-dead-helpers.md](../fixtures/05-mock-defaults-and-dead-helpers.md).
- `It.Is<T>(predicate)` for a composite argument — assert only the fields the behaviour
  depends on, so an unrelated field change does not fail an unrelated test.

The same rule governs `Setup`. A setup that matches `It.IsAny<Guid>()` cannot tell a
handler asking for the right entity from one asking for the wrong one.

Finish handler tests with `mock.VerifyNoOtherCalls()` where the set of interactions is part
of the contract — for example, that a validation failure performs **no** writes.

## 9. A boundary test asserts both sides

A single assertion at a threshold passes for many thresholds. Both sides, or it is not a
boundary test.

```csharp
[Theory]
[InlineData(UserConstants.MaxUserNameLength, true)]
[InlineData(UserConstants.MaxUserNameLength + 1, false)]
public void Validate_UserNameAtAndBeyondTheLimit_ShouldMatchTheProductionLimit(int length, bool expected)
{
    var command = new AdminCreateUserCommand(UserName: new string('a', length), /* ... */);

    _validator.TestValidate(command).IsValid.Should().Be(expected);
}
```

The lengths come from the production constant, aliased not copied. Seven such constants
have already drifted; see [01-constant-drift.md](../fixtures/01-constant-drift.md).

For counts, `Should().Be(n)` — never `BeGreaterThanOrEqualTo(n)` when the arrangement
fixes `n`. See [03-assertion-catalogue.md](03-assertion-catalogue.md).

## 10. Anything temporal goes through `TimeProvider`

`src/` currently reads the clock directly in 80 places and has no clock seam;
`tests/` contains 370 `DateTime.UtcNow` / `.Now` reads, most of them recomputing an
expected value at assertion time. That combination makes expiry, scheduling and
promotion-window logic untestable at its edges — a test cannot arrange "one second before
expiry" without sleeping.

New and touched code injects `TimeProvider` (built into .NET 8+; no package needed) and
tests supply a fake:

```csharp
// production
public class PublicRefreshTokenHandler(ISessionRepository sessions, TimeProvider clock)
{
    public async Task<PublicRefreshTokenResult> Handle(/* ... */)
    {
        if (session.ExpiresAt <= clock.GetUtcNow())
        {
            throw errors.SessionExpired();
        }
        // ...
    }
}

// test
private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero));

[Fact]
public async Task Handle_OneSecondAfterExpiry_ShouldThrowSessionExpired()
{
    SessionEntity session = SessionFactory.CreateExpiringAt(_clock.GetUtcNow());
    _sessions.SetupGetByRefreshTokenHash(session);

    _clock.Advance(TimeSpan.FromSeconds(1));

    await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
        .Should().ThrowAsync<AuthenticationException>();
}
```

`FakeTimeProvider` comes from `Microsoft.Extensions.TimeProvider.Testing`. Until a seam
exists for a given path, a test must pass explicit timestamps rather than reading the
clock; it must never assert `result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, ...)`,
which passes regardless of what the code stored. See
[unit/06](../unit/06-time-and-determinism.md).

## 11. Culture and environment are pinned with a restoring scope

`CultureInfo.CurrentUICulture` and environment variables are process- or thread-global.
A test that sets one and does not restore it corrupts whatever runs next on that worker,
and xUnit runs test classes in parallel by default.

```csharp
// bad — leaks into every subsequent test on this thread
Thread.CurrentThread.CurrentCulture = new CultureInfo("fr");
Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");

// good — restored on dispose, and scoped to the access, not the construction
using var _ = new CultureScope("fr");

errors.EmailRequired().Message.Should().Be("L'adresse e-mail est requise.");
```

The scope must wrap the **assertion**, because `IStringLocalizer` resolves at read time.
Wrapping construction is the defect documented in
[03-localizer-factory-defects.md](../fixtures/03-localizer-factory-defects.md).

Environment variables get the same treatment, and any test that mutates one joins
`EnvironmentVariableCollection` so it does not run concurrently with a test that reads it.

## 12. Reflection only to reconstitute persisted state

`tests/Unit` contains 184 reflection reads and writes into production types.
`tests/Integration` contains zero, which is the right number for a suite driving real
entry points.

Reflection is acceptable in exactly one case: setting a property that the database sets and
the domain does not expose, so that a test can start from a row that legitimately exists —
`CreatedAt`, an EF shadow navigation, an identity assigned on insert.

```csharp
// acceptable — reconstituting what the database would have written
typeof(VideoEntity)
    .GetProperty(nameof(VideoEntity.PublishedAt))!
    .SetValue(entity, _publishedAtOverride);
```

`nameof` always, never a string literal. `VideoFactory.cs:134` still uses
`GetProperty("Category", ...)`, which a rename silently breaks.

Reflection is **not** acceptable to reach a state the domain refuses to produce. That is a
test asserting against a fiction — and if the state is genuinely reachable in production,
the domain is missing a transition and `src/` is what needs the change. See
[unit/07](../unit/07-reflection-in-tests.md).

Never use reflection to invoke a private method. If it needs testing, it is either
reachable through the public surface or it belongs on a collaborator.

## Review checklist

A reviewer can decline a unit test that fails any of these:

- [ ] Would this test fail if the method body were replaced with `return default;`?
- [ ] One behaviour, one Act statement, visible AAA blocks
- [ ] Name is `Method_Scenario_ExpectedResult` and the result is an outcome, not a mechanism
- [ ] No expected value derived from the object under test
- [ ] Repeated shapes are a `[Theory]` with `TheoryData`, not copied `[Fact]`s
- [ ] Nothing mocked from the "do not mock" column
- [ ] Every `Verify` has an explicit `Times`; no `It.IsAny<>` in an identifier position
- [ ] Boundary tests assert both sides, against aliased production constants
- [ ] No `DateTime.UtcNow` in the assertion; temporal paths use `TimeProvider`
- [ ] Culture and environment changes use a restoring scope around the access
- [ ] Reflection, if any, uses `nameof` and reconstitutes persisted state only
