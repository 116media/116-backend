# High — Culture and environment leak across parallel tests

104 test files set the ambient culture on the current thread and none of them
restores it. Eight further test classes mutate process-global environment
variables outside the collection that exists to serialise exactly that. Both
categories run in parallel against tests that read the same ambient state, so the
suite contains a class of failure that depends on which test happened to run first
on which pooled thread.

## The problem

### Culture is set and never put back

The pattern appears in every localization test in the suite:

```csharp
// tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/AdminLoginValidatorTests.cs:212-230
[Theory]
[InlineData("en")]
[InlineData("fr")]
public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
{
    // Arrange
    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
    var i18n = TestErrorsFactory.CreateIdentityI18n();
    var validator = new AdminLoginValidator(i18n);
    ...
}
```

There is no `finally`, no `using`, no `IDisposable` on the class. Measured across
the 104 files that assign `Thread.CurrentThread.CurrentCulture` or
`CurrentUICulture`, **zero** contain either the token `finally` or the token
`CultureScope`. When the `fr` case of that theory finishes, the thread it ran on is
returned to the pool still set to French.

### The restoring helper already exists and is barely used

```csharp
// tests/Fixtures/Helpers/CultureScope.cs
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previous;

    public CultureScope(string cultureName)
    {
        _previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
    }

    public void Dispose()
    {
        CultureInfo.CurrentUICulture = _previous;
    }
}
```

It is referenced by four unit test files —
`tests/Unit/Shared/Exceptions/Messages/SharedExceptionMessageTests.cs` and the
three strategy tests under `tests/Unit/Shared/Exceptions/Handlers/Strategies/`.
It is also incomplete: it saves and restores `CurrentUICulture` only. The 104
leaking call sites set **both** `CurrentCulture` and `CurrentUICulture`, so a
straight substitution would fix the resource lookups and leave number and date
formatting leaking.

### Nothing serialises the tests

There is no `xunit.runner.json` anywhere in the repository and no
`[assembly: CollectionBehavior(...)]` attribute in `tests/`. xUnit therefore runs
with its default: test collections execute in parallel, each on a thread taken from
the .NET thread pool, and pool threads are reused across collections. A culture set
by a French localization test in Identity is visible to whichever unrelated test
picks that thread up next.

### Environment variables leak the same way, past a guard built for them

The suite has the right mechanism:

```csharp
// tests/Unit/Common/EnvironmentVariableCollection.cs:10
[CollectionDefinition("EnvironmentVariable", DisableParallelization = true)]
public sealed class EnvironmentVariableCollection;
```

Five classes join it: `AppEnvironmentTests`, `IdentityModuleTests`,
`ContentModuleTests`, `MailerModuleTests` and `NewsletterLinkBuilderTests`.

Eight classes mutate process-global environment variables and do **not** join it:

| Class | Variables mutated | Restores? |
| --- | --- | --- |
| `tests/Unit/Modules/Identity/Infrastructure/Services/TokenDeliveryServiceTests.cs` | `ASPNETCORE_ENVIRONMENT` (8 sites) | Yes, `Dispose` |
| `tests/Unit/Modules/Identity/Infrastructure/Services/JwtServiceTests.cs` | `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_ACCESS_TOKEN_EXPIRATION` (16 sites) | Yes, `Dispose` |
| `tests/Unit/Modules/Identity/Application/Session/Factories/SessionFactoryTests.cs:35` | `JWT_REFRESH_TOKEN_EXPIRATION` | **No** |
| `tests/Unit/.../RefreshToken/PublicRefreshTokenFactoryTests.cs:31` | `JWT_REFRESH_TOKEN_EXPIRATION` | **No** |
| `tests/Unit/.../Seeds/SuperAdmin/SuperAdminSeederTests.cs:45` | `DEFAULT_USER_PASSWORD` | Yes, `Dispose` |
| `tests/Unit/.../Seeds/SuperAdmin/SuperAdminConfigurationTests.cs:118-199` | `DEFAULT_USER_PASSWORD` | Yes, per-test `finally` |
| `tests/Unit/.../Seeds/SuperAdmin/SuperAdminSeedingStrategyTests.cs:35` | `DEFAULT_USER_PASSWORD` | **No** |
| `tests/Unit/.../Seeds/SuperAdmin/SuperAdminEntityFactoryTests.cs:31` | `DEFAULT_USER_PASSWORD` | **No** |

`TokenDeliveryServiceTests` flips `ASPNETCORE_ENVIRONMENT` between `Development`,
`Production` and `null` inside individual test bodies. `ContentModuleTests`,
`IdentityModuleTests` and `MailerModuleTests` read that same variable to decide
whether module registration enables migrations and seeding — which is precisely why
the collection exists. Because `TokenDeliveryServiceTests` is outside the
collection, it runs concurrently with all three, and `DisableParallelization` on
the collection does not help: it serialises the members against each other, not
against the rest of the assembly.

`JWT_REFRESH_TOKEN_EXPIRATION` is the clearest live race. It is read in `src` at
`src/Shared/Shared/Application/Configurations/Environment.cs:89`, set and never
cleared by `SessionFactoryTests` and `PublicRefreshTokenFactoryTests`, and set
again by the integration fixture at `tests/Integration/Common/Fixtures/ApiFixture.cs:81`.

## Why it matters

The failure mode is a test that fails on CI and passes locally, or fails once in
twenty runs, and whose stack trace points at a file that contains no bug.

A concrete instance: `AdminLoginValidatorTests` finishes its `fr` case and leaves
the thread on French. That thread is handed to a test asserting an English
validation message against a literal. `IStringLocalizer` resolves `fr`, the
message comes back in French, the assertion fails, and the reported failure is in
a file that never touched culture. The developer who investigates finds nothing
wrong with it, re-runs, gets a different thread assignment, sees green, and
concludes the suite is flaky. From that point the whole suite's failures are
discounted.

Environment-variable leakage is worse because the read happens during module
registration. A `null` `ASPNETCORE_ENVIRONMENT` left behind by
`TokenDeliveryServiceTests` can flip a module test into or out of the
migrations-and-seeding branch, so the assertion is made against a service
collection that was assembled differently from the one the test author had in mind.
That does not merely fail intermittently; when it passes, it passes for the wrong
reason.

Both categories also make the suite hostile to increased parallelism. Any future
attempt to speed up the run by widening the parallel degree increases the
collision rate, so performance work will look like it is causing bugs.

## The fix

### Step 1 — extend `CultureScope` to cover both cultures

```csharp
// Before — tests/Fixtures/Helpers/CultureScope.cs
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previous;

    public CultureScope(string cultureName)
    {
        _previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
    }

    public void Dispose()
    {
        CultureInfo.CurrentUICulture = _previous;
    }
}
```

```csharp
// After
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previousCulture;
    private readonly CultureInfo _previousUiCulture;

    /// <summary>
    /// Sets both the formatting culture and the resource-lookup culture for the
    /// duration of the scope, restoring the previous values on dispose.
    /// </summary>
    /// <param name="cultureName">
    /// The culture name to set (e.g., "en", "fr").
    /// </param>
    public CultureScope(string cultureName)
    {
        _previousCulture = CultureInfo.CurrentCulture;
        _previousUiCulture = CultureInfo.CurrentUICulture;

        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CultureInfo.CurrentCulture = _previousCulture;
        CultureInfo.CurrentUICulture = _previousUiCulture;
    }
}
```

### Step 2 — substitute at all 104 sites

The replacement is mechanical: two assignment statements become one `using`
declaration.

```csharp
// Before
Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
```

```csharp
// After
using var _ = new CultureScope(culture);
```

The `using` declaration scopes to the end of the test method, so the culture is
restored on the normal path and on the exception path alike. This is the same fix
[01-assertions-that-cannot-fail.md](01-assertions-that-cannot-fail.md) already
assumes in its localization rewrite.

### Step 3 — put the environment mutators in the collection

Add the attribute to the eight classes listed above:

```csharp
[Collection("EnvironmentVariable")]
public class TokenDeliveryServiceTests : IDisposable
```

### Step 4 — restore in the classes that do not

Three of the four leaking classes have no teardown at all. Give them one:

```csharp
// Before — tests/Unit/Modules/Identity/Application/Session/Factories/SessionFactoryTests.cs:35
public class SessionFactoryTests
{
    public SessionFactoryTests()
    {
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION", "43200");
        ...
    }
}
```

```csharp
// After
[Collection("EnvironmentVariable")]
public class SessionFactoryTests : IDisposable
{
    private readonly string? _originalRefreshExpiration;

    public SessionFactoryTests()
    {
        _originalRefreshExpiration = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION");
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION", "43200");
        ...
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION", _originalRefreshExpiration);
    }
}
```

`JwtServiceTests` at lines 41-55 already does exactly this and is the local pattern
to copy.

### Step 5 — remove the need for steps 3 and 4

Steps 3 and 4 make the tests safe; they do not make the design good. Every one of
these classes exists because `src/` reads process-global state directly:

```csharp
// src/Shared/Shared/Application/Configurations/Environment.cs:89
string? refreshTokenExpiration = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION");
```

A component that reads the environment inside its own logic cannot be configured by
its caller, which is why the tests have to reach around it and mutate the process.
Binding these variables once at startup into an options record and injecting it
removes the ambient dependency, and with it the entire category of leak:

```csharp
public sealed record JwtOptions(
    string Secret,
    string Issuer,
    string Audience,
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationMinutes
);
```

Tests then construct the options they need and the `EnvironmentVariable` collection
shrinks to the handful of tests that genuinely test environment binding itself.

## The principle

**Ambient state must be restored by the code that changed it, in a `finally`.**
Culture, environment variables, `CultureInfo.DefaultThreadCurrentCulture`, static
registries and the synchronization context are all shared by every test in the
process. A test that writes to any of them and returns has changed the meaning of
every later test.

Two rules follow:

1. **Never assign ambient state directly in a test body.** Use an `IDisposable`
   scope, so restoration is structural rather than a thing the author remembered.
   `Thread.CurrentThread.CurrentCulture = ...` in a test method is always a defect,
   even when the test is correct today.
2. **Prefer injection over the ambient value.** A `finally` makes a leak safe; a
   constructor parameter makes it impossible. The durable fix for both the culture
   and the JWT variables is that the code under test is told its configuration
   rather than discovering it.

## Checklist

- [ ] No test assigns `Thread.CurrentThread.CurrentCulture` or `CurrentUICulture`
      directly; all culture changes go through `CultureScope`.
- [ ] `CultureScope` saves and restores both `CurrentCulture` and `CurrentUICulture`.
- [ ] Every class calling `Environment.SetEnvironmentVariable` carries
      `[Collection("EnvironmentVariable")]`.
- [ ] Every such class implements `IDisposable` and restores the previous value,
      including `null`.
- [ ] New code in `src/` takes configuration as an injected options type rather than
      calling `Environment.GetEnvironmentVariable` at the point of use.
- [ ] Running the suite twice with different orderings produces the same results.
