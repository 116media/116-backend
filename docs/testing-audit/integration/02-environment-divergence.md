# Critical — A gitignored `.env` overwrites the test environment

The test fixture sets its environment, and then the application overwrites it from
a file that exists on developer machines and not in CI. The two therefore boot
materially different applications, and several tests are written against whichever
one their author happened to run.

## The problem

`ApiFixture` sets the environment before the host builds:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:66-93
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
...
Environment.SetEnvironmentVariable("JWT_SECRET", "ThisIsAVerySecureSecretKeyForTesting123!@#");
```

The application entry point then loads `.env` over the top:

```csharp
// src/Api/Program.cs:15-16
Env.Load();
Env.TraversePath().Load();
```

DotNetEnv's default `LoadOptions` is `clobberExistingVars: true`, so every value
the fixture just set is replaced. And `.env` is not in source control:

```
.gitignore:65:*.env    .env
```

It contains both keys that matter — verified: `grep -c "JWT_SECRET|ASPNETCORE_ENVIRONMENT" .env` returns 2.

The suite has already been forced to work around this, and the workaround's own
comment names the cause:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:133-135
/// Overrides JWT Bearer token validation parameters to use test constants,
/// because the production module captures env vars before test env vars are set.
```

## Why it matters

**Startup behaviour differs between machines.** Module options read the raw
variable rather than `IHostEnvironment`:

```csharp
// src/Modules/Identity/Identity/IdentityModule.cs:88-89
string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
bool enableSeeding = !environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);
```

`builder.UseEnvironment("Testing")` sets `IHostEnvironment` correctly, but nothing
reads it. Locally the raw variable is `Development` (from `.env`), so migrations and
all production seeders run at host boot. On CI they do not. Two test classes are
written on the opposite assumption — `IdentityModuleSeedingTests.cs:10-14` states
"The integration host runs under the Testing environment, where seeding is
disabled," which is true only on CI.

**The JWT secret differs**, which forces the validation override, which in turn
creates the authentication hole documented in
[03-authentication-contract-hole.md](03-authentication-contract-hole.md).

This is the class of defect where "works on my machine, fails on CI" becomes
structural rather than accidental. Worse, it is bidirectional: a test can pass
locally and fail on CI, or pass on CI and fail locally, and neither result tells
you which behaviour is correct.

## The fix

Make real environment variables authoritative. This is standard twelve-factor
behaviour: a `.env` file supplies defaults for values not already set, it does not
override the process environment.

```csharp
// src/Api/Program.cs:15-16 — before
Env.Load();
Env.TraversePath().Load();

// after
var envOptions = new LoadOptions(clobberExistingVars: false);
Env.Load(options: envOptions);
Env.TraversePath().Load(options: envOptions);
```

Then stop reading the raw variable where the framework already models it:

```csharp
// src/Modules/Identity/Identity/IdentityModule.cs — before
string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
bool enableSeeding = !environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);

// after — take IHostEnvironment through the call so UseEnvironment("Testing") is honoured
private static ModuleOptions<IdentityDbContext> GetModuleOptions(IHostEnvironment environment) =>
    new()
    {
        EnableMigrations = !environment.IsEnvironment("Testing"),
        EnableSeeding = !environment.IsEnvironment("Testing"),
    };
```

Apply the same change to the other three modules. The stale comment at
`IdentityModule.cs:87` claiming "tests use InMemory database" should go with it —
the integration tests use Testcontainers.

## What this unblocks

Fixing this is a prerequisite for three other improvements:

1. `OverrideJwtAuthentication` can be deleted, which allows a real login-token
   round trip ([03](03-authentication-contract-hole.md))
2. The two `Moq`-based module-seeding tests can be replaced with a real host that
   boots as `Development` ([04](04-production-wiring-divergence.md))
3. Local and CI runs become comparable, so a CI-only failure becomes a signal
   rather than noise

## The principle

**The test host must be the production host with only its outbound edges
replaced.** Databases point at a container, external providers are stubbed — that
is legitimate and necessary. Configuration *sources*, startup branches, and
composition are not outbound edges; when those differ, the tests are exercising an
application that never ships.

Any time a fixture has to work around production configuration, treat the
workaround as the finding. `ApiFixture`'s JWT override was a comment describing
this bug for as long as it has existed.

## Checklist

- [ ] `clobberExistingVars: false` on both `Env` load calls
- [ ] All four modules take `IHostEnvironment` instead of reading the raw variable
- [ ] `ApiFixture.OverrideJwtAuthentication` deleted
- [ ] `IdentityModuleSeedingTests` / `ContentModuleSeedingTests` replaced with a
      real Development-environment host
- [ ] A CI run and a local run produce identical results on the full suite
