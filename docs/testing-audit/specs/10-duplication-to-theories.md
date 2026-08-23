# Spec 10 — Collapse duplication clusters into theories

## Goal

Replace the largest copy-paste clusters in the unit suite with `[Theory]` methods
whose data is derived from the type system wherever the cases enumerate production
types, so that a type added to `src/` is covered without anyone remembering to copy
a test file. The suite currently holds 8,272 `[Fact]` methods against 298
`[Theory]` methods, and `[MemberData]` appears three times across 878 unit test
files. The measured duplication is 1,643 facts in 572 near-identical clusters; this
spec takes the five largest clusters, which account for 303 facts across 17 files.

The motivating evidence is not verbosity. `tests/Unit/Modules/Content/Infrastructure/Cache/`
contains exactly two test files, `PopularArticlesCacheInvalidatorTests.cs` and
`PopularTagsCacheInvalidatorTests.cs`, and normalising the type name in one produces
the other byte for byte. `src/Modules/Content/Content/Infrastructure/Cache/` contains
three concrete invalidators: `PopularArticlesCacheInvalidator`,
`PopularTagsCacheInvalidator` and `PopularVideosCacheInvalidator`. The third has no
test file. A mock for it exists at
`tests/Unit/Common/Mocks/Infrastructure/MockPopularVideosCacheInvalidator.cs`, so the
type was known to whoever wrote the second copy. The second file was produced by
find-and-replace; the third was not produced, because producing it required someone
to remember. A theory sourced from the assembly's `CacheInvalidator` subclasses would
have covered it the moment it was written.

## Scope

In scope:

- `tests/Unit/Modules/Content/Infrastructure/Persistence/ContentDbContextTests.cs`
  (50 facts, 451 lines).
- `tests/Unit/Modules/Identity/Application/Shared/Errors/UserErrorsTests.cs`
  (51 facts, 531 lines).
- The 13 files in `tests/Unit/Shared/Exceptions/Handlers/Strategies/` (117 facts).
- `tests/Unit/Modules/Identity/Application/Auth/Validators/CredentialValidationTests.cs`
  (47 facts, 655 lines).
- `tests/Unit/Modules/Core/Infrastructure/Services/CloudinaryServiceTests.cs`
  (38 facts).
- `tests/Unit/Modules/Content/Infrastructure/Cache/` (two files, 12 facts, replaced
  by one file with an assembly-sourced theory).

Not in this spec:

- The remaining 567 clusters. They are covered by the review habit in
  [../unit/05-duplication-and-theories.md](../unit/05-duplication-and-theories.md),
  not by a bulk rewrite. Collapsing a cluster is only worth doing when the resulting
  theory asserts at least what the facts asserted.
- The 104 localization tests. Those are spec 06, and the correct replacement there is
  a resource-completeness theory rather than a data-driven restatement.
- Any change to `src/`. Every file listed above is under `tests/`.
- The integration suite. Its one data-driven collapse is the rate limit policy theory
  in spec 12.

## Prerequisites

- Spec 02 has landed, so `CultureScope` covers both `CurrentCulture` and
  `CurrentUICulture`. `NotFoundExceptionHandlerTests` already uses `CultureScope` for
  its two French cases, and those cases move into the cross-file theory in Change 3.
- Spec 03 has landed, so `CredentialValidationTests` reads its boundary values from
  `UserConstants` rather than from local literals. Change 4 assumes the constants are
  already aliased; collapsing the file first would bake the drifted literals into
  theory rows.
- Spec 05 has landed, so `NotBeNull`-only assertions have already been replaced where
  a real outcome was available. Change 1 depends on this: the point of the
  `ContentDbContext` theory is that it asserts mapping, not non-nullness.

## Changes

### 1. Collapse `ContentDbContextTests` into a theory over the domain's entity types

`ContentDbContext` declares 49 `DbSet` properties. The test file has 47 facts of a
single shape plus three facts covering schema and configuration. The 47 assert
something Entity Framework Core guarantees for any declared `DbSet`, and each one
constructs a fresh in-memory provider to do it.

Before, at `tests/Unit/Modules/Content/Infrastructure/Persistence/ContentDbContextTests.cs:22-35`
and repeated 45 more times:

```csharp
[Fact]
public void ContentTypes_ShouldReturnDbSet()
{
    using var context = new ContentDbContext(CreateOptions());
    DbSet<ContentTypeEntity> result = context.ContentTypes;
    result.Should().NotBeNull();
}

[Fact]
public void PricingTiers_ShouldReturnDbSet()
{
    using var context = new ContentDbContext(CreateOptions());
    DbSet<PricingTierEntity> result = context.PricingTiers;
    result.Should().NotBeNull();
}
```

After, one theory whose rows come from the domain assembly:

```csharp
/// <summary>
/// Supplies every concrete domain entity type declared by the Content module, so that a
/// newly added entity becomes a theory row without any change to this file.
/// </summary>
/// <returns>The domain entity types, ordered by name for stable test output.</returns>
public static TheoryData<Type> DomainEntities() =>
    new(
        typeof(ContentTypeEntity)
            .Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true } && t.Name.EndsWith("Entity"))
            .OrderBy(t => t.Name)
    );

[Theory]
[MemberData(nameof(DomainEntities))]
public void Model_ShouldMapEveryDomainEntityWithAPrimaryKey(Type entityType)
{
    using var context = new ContentDbContext(CreateOptions());

    IEntityType? mapped = context.Model.FindEntityType(entityType);

    mapped.Should().NotBeNull($"{entityType.Name} is a domain entity and must be mapped");
    mapped!.FindPrimaryKey().Should().NotBeNull($"{entityType.Name} must declare a primary key");
}
```

Keep the three facts in the `Schema and Configuration` region
(`ContentDbContextTests.cs:404` onwards) unchanged. They assert the default schema and
that configurations are applied from the assembly, which the theory does not cover.

What breaks if done wrong: writing the theory over 47 property names instead of over
entity types preserves the copy-paste in a different syntax and asserts nothing new.
The value of this change is entirely in the data source. Two of the 49 `DbSet`
properties have no fact today, and the theory covers them the moment it lands; if
either fails, that is the gap the facts were hiding, not a defect introduced here.

### 2. Collapse the `UserErrors` factory clusters into three theories

`UserErrorsTests` has 51 facts. Forty-six follow one of four shapes grouped by the
exception the factory returns: 17 conflict, 19 bad request, and 10 across the
authentication, authorization and OTP families. The remaining five cover the message
properties and the localizer directly and stay as facts.

Before, at `tests/Unit/Modules/Identity/Application/Shared/Errors/UserErrorsTests.cs:33-59`:

```csharp
[Fact]
public void EmailAlreadyExists_ShouldReturnConflictException()
{
    // Arrange
    const string email = "user@example.com";

    // Act
    ConflictException exception = _errors.EmailAlreadyExists(email);

    // Assert
    exception.Should().BeOfType<ConflictException>();
    exception.Message.Should().Be(_conflict.EmailAlreadyExists(email));
}

[Fact]
public void UsernameAlreadyExists_ShouldReturnConflictException()
{
    // Arrange
    const string username = "john_doe";

    // Act
    ConflictException exception = _errors.UsernameAlreadyExists(username);

    // Assert
    exception.Should().BeOfType<ConflictException>();
    exception.Message.Should().Be(_conflict.UsernameAlreadyExists(username));
}
```

After, one theory per exception family carrying factory delegates as rows:

```csharp
/// <summary>
/// Supplies the single-argument conflict factories on <see cref="UserErrors" /> paired with
/// the localized message each one is required to carry. The first element names the case so
/// the runner reports a readable identifier instead of a delegate's type name.
/// </summary>
/// <returns>Case name, argument value, error factory, and expected message factory per row.</returns>
public static TheoryData<
    string,
    string,
    Func<UserErrors, string, BaseException>,
    Func<ConflictErrorMessage, string, string>
> ConflictFactories() =>
    new()
    {
        {
            nameof(UserErrors.EmailAlreadyExists),
            "user@example.com",
            (e, v) => e.EmailAlreadyExists(v),
            (m, v) => m.EmailAlreadyExists(v)
        },
        {
            nameof(UserErrors.UsernameAlreadyExists),
            "john_doe",
            (e, v) => e.UsernameAlreadyExists(v),
            (m, v) => m.UsernameAlreadyExists(v)
        },
        {
            nameof(UserErrors.PhoneNumberAlreadyExists),
            "+1234567890",
            (e, v) => e.PhoneNumberAlreadyExists(v),
            (m, v) => m.PhoneNumberAlreadyExists(v)
        },
    };

[Theory]
[MemberData(nameof(ConflictFactories))]
public void ConflictFactory_ShouldReturnConflictExceptionWithLocalizedMessage(
    string caseName,
    string value,
    Func<UserErrors, string, BaseException> factory,
    Func<ConflictErrorMessage, string, string> expected
)
{
    BaseException exception = factory(_errors, value);

    exception.Should().BeOfType<ConflictException>(caseName);
    exception.Message.Should().Be(expected(_conflict, value), caseName);
}
```

Two details are load-bearing. First, the declared type of `exception` is
`BaseException`, not `ConflictException`, which turns `BeOfType<ConflictException>()`
from a restatement of the compiler's knowledge into a runtime check. Second, the
`caseName` parameter exists because xUnit derives a theory case's display name from
its arguments, and a `Func<,,>` renders as its type name. Without the name parameter
a failing row reports as
`ConflictFactory_ShouldReturnConflictExceptionWithLocalizedMessage(caseName: ..., factory: Func\`3, ...)`
for every row, and the reader cannot tell which factory failed.

Repeat the same shape for `BadRequestFactories` (19 rows returning
`BadRequestException`, expected messages from `_validation`) and for
`AuthFactories`, which covers the six authentication, authorization and OTP factories
whose exception types differ per row. `AuthFactories` therefore carries the expected
`Type` as a column:

```csharp
/// <summary>
/// Supplies the authentication, authorization, and OTP factories together with the concrete
/// exception type each is required to produce, since these families do not share one type.
/// </summary>
/// <returns>Case name, expected exception type, and error factory per row.</returns>
public static TheoryData<string, Type, Func<UserErrors, BaseException>> AuthFactories() =>
    new()
    {
        { nameof(UserErrors.AccountInactive), typeof(AccountInactiveException), e => e.AccountInactive() },
        { nameof(UserErrors.AccountNotVerified), typeof(AccountNotVerifiedException), e => e.AccountNotVerified() },
        { nameof(UserErrors.InvalidCredentials), typeof(AuthenticationException), e => e.InvalidCredentials() },
    };
```

What breaks if done wrong: a theory that asserts only `exception.Should().NotBeNull()`
covers less than the 17 facts it replaces. Every row must still assert both the
exception type and the exact localized message, or the collapse loses coverage.

### 3. Collapse the 13 exception-handler strategy files into cross-file theories

`tests/Unit/Shared/Exceptions/Handlers/Strategies/` contains 13 files holding 117
facts. Nine files hold seven or eight facts each and differ only in which exception
type and status code they exercise. Because the duplication is across files, nobody
reading a single file sees it.

Every strategy derives from `BaseExceptionStrategy<TException>`
(`src/Shared/Shared/Application/Exceptions/Handlers/Contracts/BaseExceptionStrategy.cs`),
which supplies `ExceptionType` and routes through `CreateStandardProblemDetails`. That
common base is exactly what the shared facts assert, so it collapses into one file
holding the common theories, with the per-strategy files retaining only what is
genuinely specific.

Create `tests/Unit/Shared/Exceptions/Handlers/Strategies/ExceptionStrategyContractTests.cs`:

```csharp
/// <summary>
/// Supplies every concrete <see cref="IExceptionStrategy" /> in the Shared assembly, paired
/// with an instance of the exception it declares and the status code its ProblemDetails must
/// carry. A strategy added to the assembly becomes a theory row with no change to this file.
/// </summary>
/// <returns>Strategy instance, sample exception, and expected status code per row.</returns>
public static TheoryData<IExceptionStrategy, Exception, int> Strategies()
{
    var data = new TheoryData<IExceptionStrategy, Exception, int>();

    foreach (IExceptionStrategy strategy in DiscoverStrategies())
    {
        data.Add(strategy, ExceptionSamples.For(strategy.ExceptionType), ExpectedStatus.For(strategy.ExceptionType));
    }

    return data;
}

[Theory]
[MemberData(nameof(Strategies))]
public void CreateProblemDetails_ShouldProduceTheStandardEnvelope(
    IExceptionStrategy strategy,
    Exception exception,
    int expectedStatus
)
{
    DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
    context.Request.Path = "/api/v1/admin/users/123";
    context.TraceIdentifier = "trace-for-" + strategy.ExceptionType.Name;

    ProblemDetails problem = strategy.CreateProblemDetails(exception, context);

    problem.Title.Should().Be(strategy.ExceptionType.Name);
    problem.Status.Should().Be(expectedStatus);
    problem.Instance.Should().Be("/api/v1/admin/users/123");
    problem.Extensions["traceId"].Should().Be(context.TraceIdentifier);
    problem.Extensions.Should().ContainKey("timestamp");
}

[Theory]
[MemberData(nameof(Strategies))]
public void ExceptionType_ShouldMatchTheStrategyGenericArgument(
    IExceptionStrategy strategy,
    Exception exception,
    int expectedStatus
)
{
    _ = expectedStatus;

    strategy.ExceptionType.Should().Be(exception.GetType());
}
```

`DiscoverStrategies` scans the Shared assembly for non-abstract types assignable to
`IExceptionStrategy` and activates each with its parameterless constructor.
`ExceptionSamples` and `ExpectedStatus` are private lookups in the same file, keyed on
exception type; a strategy with no entry must fail the theory rather than be skipped,
so both throw with the type name when the key is absent. That is what makes the new
strategy case loud instead of silent.

After this change, the per-strategy files keep only their non-shared facts:

- `NotFoundExceptionHandlerTests` keeps its eight facts covering the friendly
  localized entity message, the two leak assertions, the unmapped-entity fallback, and
  the two French cases.
- `ValidationExceptionHandlerTests` keeps the facts covering the errors extension.
- `RateLimitExceededExceptionHandlerTests` keeps the facts covering `Retry-After`.
- `FormatExceptionStrategyTests` keeps the facts covering message rewriting.
- The remaining nine files delete their common facts entirely; where nothing specific
  remains, delete the file.

Expected result is 117 facts down to roughly 40 facts plus 5 theories.

What breaks if done wrong: if `DiscoverStrategies` silently skips a type it cannot
activate, a strategy added with a constructor dependency drops out of coverage without
any signal. The scan must throw on a type it cannot construct.

### 4. Collapse `CredentialValidationTests` into nine theories

The file holds 47 facts across eight validator extensions, and the facts within each
extension differ only in the input string and the expected message. The private test
command types and their validators stay as they are.

Before, at `tests/Unit/Modules/Identity/Application/Auth/Validators/CredentialValidationTests.cs:119-150`:

```csharp
[Fact]
public void ValidEmail_WithNullEmail_ShouldFail()
{
    var validator = new TestEmailCommandValidator(_enMsg);
    var command = new TestEmailCommand { Email = null };

    TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

    result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_enMsg.EmailRequired());
}

[Fact]
public void ValidEmail_WithEmptyEmail_ShouldFail()
{
    var validator = new TestEmailCommandValidator(_enMsg);
    var command = new TestEmailCommand { Email = string.Empty };

    TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

    result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_enMsg.EmailRequired());
}
```

After, one rejecting theory and one accepting theory per rule:

```csharp
/// <summary>
/// Supplies the inputs the required-email rule must reject, together with the message key
/// that identifies which of the rule's three failure branches is expected.
/// </summary>
/// <returns>Candidate email and expected failure branch per row.</returns>
public static TheoryData<string?, EmailFailure> RejectedEmails() =>
    new()
    {
        { null, EmailFailure.Required },
        { string.Empty, EmailFailure.Required },
        { "   ", EmailFailure.Required },
        { new string('a', UserConstants.MaxEmailLength) + "@example.com", EmailFailure.TooLong },
        { "not-an-email", EmailFailure.Format },
        { "userexample.com", EmailFailure.Format },
    };

[Theory]
[MemberData(nameof(RejectedEmails))]
public void ValidEmail_Required_ShouldRejectWithTheExpectedMessage(string? email, EmailFailure failure)
{
    var validator = new TestEmailCommandValidator(_enMsg);
    var command = new TestEmailCommand { Email = email };

    TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

    result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(ExpectedMessage(failure));
}
```

`EmailFailure` is a private enum in the file and `ExpectedMessage` maps it to
`_enMsg.EmailRequired()`, `_enMsg.EmailTooLong(UserConstants.MaxEmailLength)` or
`_enMsg.InvalidEmailFormatMsg()`. Carrying the branch rather than the raw string keeps
the assertion tied to the localizer, which is what makes an emptied resource entry
fail the test.

The nine theories are: required email rejects, required email accepts, optional email,
strong password, non-strong password, required username, optional username,
credentials, and old password. The two cascade facts
(`ValidPassword_WithEmptyPassword_ShouldStopCascading` and its non-strong twin) stay as
facts, because they assert error *count*, not a message, and folding them into a
theory would lose that.

What breaks if done wrong: replacing `WithErrorMessage(...)` with a bare
`ShouldHaveValidationErrorFor(...)` makes the theory pass for any failure branch, so a
rule that starts reporting "too long" for a malformed address would go unnoticed. The
branch assertion is the part worth keeping.

### 5. Collapse `CloudinaryServiceTests` into one fact and eight theories

The file holds 38 facts, 32 of which repeat one of two shapes per upload method:
a rejected input and an accepted input. All three upload methods take the same
`(IFormFile, string publicId)` shape, so the theory can range over method delegates as
well as inputs.

```csharp
/// <summary>
/// Supplies the three upload entry points paired with a file whose extension and content type
/// the method must reject, so that a new upload method is added as a row rather than as a copy
/// of the surrounding facts.
/// </summary>
/// <returns>Case name, upload invocation, file name, and content type per row.</returns>
public static TheoryData<string, Func<ICloudinaryService, IFormFile, Task>, string, string?> RejectedUploads() =>
    new()
    {
        { "image/exe", (s, f) => s.UploadImageAsync(f, "test-id"), "malware.exe", "application/x-msdownload" },
        { "raw/exe", (s, f) => s.UploadRawAsync(f, "test-id"), "malware.exe", "application/x-msdownload" },
        { "video/exe", (s, f) => s.UploadVideoAsync(f, "test-id"), "malware.exe", "application/x-msdownload" },
    };

[Theory]
[MemberData(nameof(RejectedUploads))]
public async Task Upload_WithDisallowedFile_ShouldThrowBadRequestException(
    string caseName,
    Func<ICloudinaryService, IFormFile, Task> upload,
    string fileName,
    string? contentType
)
{
    var service = new CloudinaryService(_settings, _loggerMock.Object, TestErrorsFactory.CreateCoreI18n());
    IFormFile file = FormFileBuilder.WithName(fileName).WithContentType(contentType).WithLength(1024).Build();

    Func<Task> act = () => upload(service, file);

    await act.Should().ThrowExactlyAsync<BadRequestException>(caseName);
}
```

The eight theories are: disallowed extension, disallowed content type, null file,
empty file, oversize file, accepted extensions per method, content-type
normalisation (uppercase extension, mixed-case content type, parameters after the
media type), and the tolerated content types (`null`, empty, `application/octet-stream`,
`multipart/form-data`). `Constructor_WithValidSettings_ShouldNotThrow` stays a fact.

`FormFileBuilder` replaces the repeated `Mock<IFormFile>` arrangement; it belongs in
`tests/Fixtures/` under the layering rule in spec 08, and this change is the first
consumer. If spec 08 has not landed, put it in the test file as a private static
helper and move it when spec 08 does.

What breaks if done wrong: the current facts use a mix of `ThrowExactlyAsync` and
`ThrowAsync`. Standardise on `ThrowExactlyAsync<BadRequestException>` in the theory —
`ThrowAsync` also passes for any subclass, which would let a change that starts
throwing a derived type slip through.

### 6. Replace the two invalidator test files with one assembly-sourced theory

Delete `PopularArticlesCacheInvalidatorTests.cs` and `PopularTagsCacheInvalidatorTests.cs`
and add `tests/Unit/Modules/Content/Infrastructure/Cache/CacheInvalidatorTests.cs`:

```csharp
/// <summary>
/// Supplies every concrete <see cref="CacheInvalidator" /> in the Content infrastructure
/// assembly. A domain invalidator added to the module is covered by this theory without any
/// change here, which is the property the two hand-copied test files did not have.
/// </summary>
/// <returns>The concrete invalidator types, ordered by name for stable test output.</returns>
public static TheoryData<Type> Invalidators() =>
    new(
        typeof(CacheInvalidator)
            .Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICacheInvalidator).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
    );

[Theory]
[MemberData(nameof(Invalidators))]
public void Invalidate_ShouldCancelThePreviousTokenAndIssueALiveOne(Type invalidatorType)
{
    var invalidator = (ICacheInvalidator)Activator.CreateInstance(invalidatorType)!;

    CancellationToken before = invalidator.GetEvictionToken();
    invalidator.Invalidate();
    CancellationToken after = invalidator.GetEvictionToken();

    before.IsCancellationRequested.Should().BeTrue($"{invalidatorType.Name} must evict entries on invalidation");
    after.IsCancellationRequested.Should().BeFalse($"{invalidatorType.Name} must accept new cache fills");
    after.Should().NotBe(before);
}
```

Carry over the remaining behaviours from the deleted files as theories over the same
`Invalidators()` source: the token is stable across two `GetEvictionToken` calls, and
two successive `Invalidate` calls cancel both earlier tokens. Six facts per file
become three theories covering three types, and `PopularVideosCacheInvalidator` is
covered for the first time.

Add one assertion that the theory itself has not gone empty, because a reflection
query that matches nothing produces a silently passing theory with zero cases:

```csharp
[Fact]
public void Invalidators_ShouldDiscoverEveryConcreteInvalidator()
{
    Invalidators().Count.Should().Be(3, "the Content module declares three domain cache invalidators");
}
```

Update that count deliberately when an invalidator is added; the failure is the
notification that a new type needs a registration review.

What breaks if done wrong: `Activator.CreateInstance` on a type with constructor
dependencies throws, and the theory would report a construction error rather than a
behavioural one. That is acceptable and correct — it means the invalidator hierarchy
gained a dependency and this test needs to resolve it from DI instead.

## Expected fallout

- Two of the 49 `ContentDbContext` `DbSet` properties have no fact today. If either
  entity is unmapped or lacks a primary key, Change 1 turns red on first run. Fix the
  configuration, not the theory.
- `PopularVideosCacheInvalidator` is exercised for the first time in Change 6. It
  inherits all behaviour from `CacheInvalidator`, so a failure would indicate a
  genuine difference rather than a test problem.
- Change 3 may reveal a strategy whose `ExpectedStatus` entry nobody can name, which
  means the strategy is registered but its status code was never asserted anywhere.
  Add the entry and record the status in the spec's implementation notes.
- Test counts move sharply. Facts drop by roughly 250 across the six changes and
  theory methods rise by roughly 30, but theory *cases* rise by more than the facts
  removed, because the assembly-sourced theories cover types no fact covered. Do not
  treat the fact-count drop as lost coverage in the PR description; state the case
  count.
- CI duration for the unit suite drops measurably in Change 1 alone: 47 in-memory
  provider constructions become one per theory case, against a smaller set.

## Testing

```bash
dotnet build
dotnet test tests/Unit
dotnet test tests/Unit --filter "FullyQualifiedName~ContentDbContextTests"
dotnet test tests/Unit --filter "FullyQualifiedName~CacheInvalidatorTests"
dotnet test tests/Unit --filter "FullyQualifiedName~ExceptionStrategyContractTests"
```

The whole unit suite must be green. The integration suite is untouched by this spec
but must still be run once before merge, because Change 3 deletes files whose types
the integration suite may reference.

What the new tests prove that the old ones did not:

- `Model_ShouldMapEveryDomainEntityWithAPrimaryKey` fails when an entity is added to
  `src/Modules/Content/Content/Domain/Entities/` and never configured. Prove it by
  adding a throwaway `ProbeEntity` class to the domain assembly, confirming the theory
  goes red, then removing it.
- `Invalidate_ShouldCancelThePreviousTokenAndIssueALiveOne` covers
  `PopularVideosCacheInvalidator`, which had no test. Prove it by checking the
  runner output lists three cases.
- `ExceptionStrategyContractTests` fails when a new `IExceptionStrategy` is added
  without an expected-status entry. Prove it by adding a stub strategy in a scratch
  branch and confirming the discovery throws.

## Risks

**A theory can hide a shrinking data source.** A reflection query that matches nothing
produces a passing theory with zero executed cases, and nothing in the run report
distinguishes that from a passing theory with 40 cases. Mitigation: every
assembly-sourced theory in this spec is paired with a count fact, as shown in Change 6.
Apply the same pairing to Changes 1 and 3.

**Delegate rows produce unreadable failures.** `TheoryData` carrying `Func<>` columns
renders as the delegate's type name in the runner. Mitigation: every such theory takes a
leading `caseName` string, and every assertion passes it as the `because` argument, so
the failure message names the case even when the display name does not.

**Collapsing can quietly weaken assertions.** The reviewer's temptation is to keep only
the assertions all N facts had in common. Mitigation: the ground rule in
[00-index.md](00-index.md) applies per row — for each row, state what change to `src/`
would break it. Changes 2, 4 and 5 each name the assertion that must survive.

**Change 3 spans 13 files and one shared base.** A mistake there affects the whole
error-response contract's unit coverage. Mitigation: land Change 3 as its own commit,
and diff the per-strategy files to confirm each retained fact is genuinely specific
before deleting the common ones.

**`Activator.CreateInstance` is reflection, which spec 08 is reducing.** The
distinction the codebase draws is between reflection used to reach private state, which
is a defect, and reflection used to enumerate the type system, which is the point of
this spec. Mitigation: state that distinction in the doc comment on each
`[MemberData]` factory, and never use reflection in these theories to set a field.

## Checklist

- [ ] 1 — `ContentDbContextTests` collapsed to one entity-type theory plus the three
      schema and configuration facts, with a count fact guarding the data source
- [ ] 2 — `UserErrorsTests` collapsed to three factory theories plus the five
      non-factory facts, each row carrying a case name and asserting type and message
- [ ] 3 — `ExceptionStrategyContractTests` added, the nine common facts removed from
      each strategy file, and files with nothing specific left deleted
- [ ] 4 — `CredentialValidationTests` collapsed to nine theories plus the two cascade
      facts, with expected messages read from the localizer
- [ ] 5 — `CloudinaryServiceTests` collapsed to one fact plus eight theories, using
      `ThrowExactlyAsync` throughout
- [ ] 6 — The two invalidator test files replaced by one assembly-sourced theory file
      covering all three invalidators, with a count fact
- [ ] Suite totals recorded in the PR: facts before and after, theory methods before
      and after, and theory *cases* after
