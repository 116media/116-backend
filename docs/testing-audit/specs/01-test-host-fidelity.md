# Spec 01 — Test host fidelity

## Goal

The integration host is meant to be the production host with only its outbound edges
replaced. Today it is not: a live Quartz scheduler mutates the same four schemas the
tests assert on, a gitignored `.env` overwrites the environment the fixture just set,
the fixture downgrades four pooled DbContext registrations to unpooled ones, and the
JWT validation parameters the application configured are discarded before any request
runs. This spec brings the host back in line with production. It is first because
every other result in the audit is measured against this host — until local and CI
boot the same application and nothing mutates rows on a timer, a green run proves
nothing in particular and a red run cannot be attributed.

Backing findings: [../integration/01-background-jobs-in-the-test-host.md](../integration/01-background-jobs-in-the-test-host.md),
[../integration/02-environment-divergence.md](../integration/02-environment-divergence.md),
[../integration/03-authentication-contract-hole.md](../integration/03-authentication-contract-hole.md),
[../integration/04-production-wiring-divergence.md](../integration/04-production-wiring-divergence.md).

## Scope

In this spec:

- `ApiFixture` stops registering the Quartz hosted service, while leaving every job
  and trigger definition and `ISchedulerFactory` in the container.
- `src/Api/Program.cs` loads `.env` without clobbering variables that are already set
  in the process.
- `IdentityModule`, `ContentModule`, `MailerModule` and `CoreModule` take
  `IHostEnvironment` instead of reading `ASPNETCORE_ENVIRONMENT` from the process.
- `ApiFixture.ReplaceDbContext` re-registers each context with `AddDbContextPool` and
  an explicit `AddInterceptors` call, and removes the pool descriptors that the
  current replacement orphans.
- `ApiFixture.OverrideJwtAuthentication` is deleted, and one integration test logs in
  and calls a protected endpoint with the token the application issued.
- `ApiFixture` gains a `protected virtual string EnvironmentName`, and the two
  `Moq`-based module-seeding tests are replaced by a fixture that boots as
  `Development`.

Not in this spec:

- DI scope disposal in the integration base classes. That is
  [02-test-isolation.md](02-test-isolation.md) Change 2, and **it must land before
  Change 3 here**.
- Resetting the external-service stubs, and the `stub.Sent.Count` baseline reads that
  the live scheduler forced — [02-test-isolation.md](02-test-isolation.md) Change 1.
- Replacing `Environment.GetEnvironmentVariable` in `src/` with an injected options
  record. The environment reads in `AppEnvironment` and `TokenDeliveryService` stay as
  they are; only the four `GetModuleOptions` methods change.
- Strengthening error assertions to pin the ProblemDetails `Title` and `Detail` —
  [04-error-assertion-discipline.md](04-error-assertion-discipline.md).
- Container count and runtime. Change 5 adds a third container; whether the suite keeps
  three is a question for spec 11.

## Prerequisites

None for Changes 1, 2 and 5.

Change 3 requires [02-test-isolation.md](02-test-isolation.md) Change 2 to have landed
first. `BaseApiTest.CreateDbContext`, `BaseRepositoryTest.CreateDbContext`,
`BaseRepositoryTest.Resolve` and `BaseRepositoryTest.CreateScopedRepository` each open
an `IServiceScope` and drop the reference, roughly 1,189 times per run. A pooled
context is returned to the pool when its scope is disposed. Restoring
`AddDbContextPool` while those scopes are abandoned means pooled contexts are checked
out and never returned, and the suite exhausts the pool instead of gaining fidelity.
**Do not implement Change 3 until scope disposal is merged.**

Change 4 requires Change 2 in this spec. The host cannot validate a token it issued
with the fixture's secret until `.env` stops overwriting `JWT_SECRET` after the fixture
sets it.

## Changes

### 1. Remove the Quartz hosted service from the test host

File: `tests/Integration/Common/Fixtures/ApiFixture.cs`.

`AddScheduledJob` registers the hosted service on every call
(`src/Shared/Shared/Application/Extensions/QuartzExtension.cs:38`), and four jobs are
scheduled against it — `OutboxEmailDispatcherJob` every fifteen seconds
(`src/Modules/Mailer/Mailer/MailerModule.cs:73`), `ExpiredOtpCleanupJob` hourly,
`AbandonedDraftCleanupJob` and `ShortVideoViewEventCleanupJob`. The fixture removes no
`IHostedService` today.

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:49-59 — before
builder.ConfigureTestServices(services =>
{
    ReplaceDbContexts(services);
    StubExternalServices(services);
    OverrideJwtAuthentication(services);

    if (DisableRateLimits)
    {
        DisableRateLimiting(services);
    }
});
```

```csharp
// after
builder.ConfigureTestServices(services =>
{
    ReplaceDbContexts(services);
    StubExternalServices(services);
    DisableScheduledJobs(services);

    if (DisableRateLimits)
    {
        DisableRateLimiting(services);
    }
});

/// <summary>
/// Removes the Quartz hosted service so that no scheduled trigger fires while a test is
/// running. Every job definition, trigger definition and <see cref="ISchedulerFactory" />
/// registration stays in the container, so the job-registration assertions still resolve a
/// scheduler that knows every job key; only the component that fires triggers on a timer is
/// gone. Job behaviour itself stays covered by the tests that resolve the job's real
/// collaborators and invoke <c>Execute</c> once.
/// </summary>
/// <param name="services">The test host's service collection.</param>
private static void DisableScheduledJobs(IServiceCollection services)
{
    List<ServiceDescriptor> hostedServices = services
        .Where(descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
            && descriptor.ImplementationType?.Name == "QuartzHostedService"
        )
        .ToList();

    foreach (ServiceDescriptor descriptor in hostedServices)
    {
        services.Remove(descriptor);
    }
}
```

`OverrideJwtAuthentication` disappears from the delegate in Change 4; it is shown
removed here only so the final shape of the method is unambiguous. If Change 4 is
deferred, keep the call.

Add `using Microsoft.Extensions.Hosting;` for `IHostedService`.

**If this is done wrong** — by removing `AddQuartz` or `ISchedulerFactory` rather than
only the hosted service — the two registration assertions at
`tests/Integration/Modules/Content/Infrastructure/BackgroundJobs/AbandonedDraftCleanupJobTests.cs:42-50`
and
`tests/Integration/Modules/Identity/Infrastructure/BackgroundJobs/ExpiredOtpCleanupJobTests.cs:38-46`
stop proving that the module schedules its job, and a module that silently stopped
scheduling would ship green.

### 2. Make `.env` non-clobbering and read `IHostEnvironment`

Files: `src/Api/Program.cs`, `src/Modules/Identity/Identity/IdentityModule.cs`,
`src/Modules/Content/Content/ContentModule.cs`,
`src/Modules/Mailer/Mailer/MailerModule.cs`, `src/Modules/Core/Core/CoreModule.cs`.

DotNetEnv defaults to `clobberExistingVars: true`, so `.env` replaces every value
`ApiFixture.SetEnvironmentVariables` sets at `ApiFixture.cs:66-93`. A `.env` exists on
developer machines and not in CI, and it carries both `ASPNETCORE_ENVIRONMENT` and
`JWT_SECRET`.

```csharp
// src/Api/Program.cs:15-16 — before
Env.Load();
Env.TraversePath().Load();
```

```csharp
// after
var envOptions = new LoadOptions(clobberExistingVars: false);
Env.Load(options: envOptions);
Env.TraversePath().Load(options: envOptions);
```

Then take the environment through the module options instead of reading the raw
variable. `IdentityModule.cs:85-98` is the template; `ContentModule.cs:53-65` and
`MailerModule.cs:32-44` are the same shape.

```csharp
// src/Modules/Identity/Identity/IdentityModule.cs:85-98 — before
/// <summary>
/// Gets the shared module configuration options for the Identity module.
/// </summary>
private static ModuleOptions<IdentityDbContext> GetModuleOptions()
{
    // Disable migrations and seeding in Testing environment (tests use InMemory database)
    string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    bool enableSeeding = !environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);

    return new ModuleOptions<IdentityDbContext>
    {
        ModuleName = IdentityConstants.ModuleName,
        SchemaName = IdentityConstants.SchemaName,
        EnableMigrations = enableSeeding,
        EnableSeeding = enableSeeding,
    };
}
```

```csharp
// after
/// <summary>
/// Gets the shared module configuration options for the Identity module.
/// Migrations and seeding run everywhere except the Testing environment, where the
/// integration fixture applies migrations itself and seeds only what a test arranges.
/// </summary>
/// <param name="environment">
/// The host environment, so that <c>UseEnvironment</c> is honoured rather than the raw
/// process variable, which a <c>.env</c> file may have replaced.
/// </param>
private static ModuleOptions<IdentityDbContext> GetModuleOptions(IHostEnvironment environment)
{
    bool enableSeeding = !environment.IsEnvironment("Testing");

    return new ModuleOptions<IdentityDbContext>
    {
        ModuleName = IdentityConstants.ModuleName,
        SchemaName = IdentityConstants.SchemaName,
        EnableMigrations = enableSeeding,
        EnableSeeding = enableSeeding,
    };
}
```

The stale comment claiming "tests use InMemory database" goes with it: the integration
tests use Testcontainers.

Both callers must supply the environment. `Add*Module` takes it as a parameter,
`Use*Module` resolves it from the application services:

```csharp
// src/Modules/Identity/Identity/IdentityModule.cs:114-116 — before
public static IServiceCollection AddIdentityModule(this IServiceCollection services)
{
    services.AddModuleDatabase(GetModuleOptions());
```

```csharp
// after
/// <param name="environment">The host environment deciding migration and seeding behaviour.</param>
public static IServiceCollection AddIdentityModule(this IServiceCollection services, IHostEnvironment environment)
{
    services.AddModuleDatabase(GetModuleOptions(environment));
```

```csharp
// src/Modules/Identity/Identity/IdentityModule.cs:247-249 — before
public static IApplicationBuilder UseIdentityModule(this IApplicationBuilder app)
{
    ModuleOptions<IdentityDbContext> options = GetModuleOptions();
```

```csharp
// after
public static IApplicationBuilder UseIdentityModule(this IApplicationBuilder app)
{
    IHostEnvironment environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
    ModuleOptions<IdentityDbContext> options = GetModuleOptions(environment);
```

`CoreModule.GetModuleOptions` (`CoreModule.cs:28-35`) does not branch on the
environment at all — it hardcodes `EnableMigrations = true` and `EnableSeeding = false`.
Give it the same signature for symmetry and keep its values unchanged:

```csharp
// src/Modules/Core/Core/CoreModule.cs:28-35 — after
/// <summary>
/// Gets the shared module configuration options for the Core module. Core owns no seeders,
/// and its migrations run in every environment; the parameter exists so that all four
/// modules take their environment from the same source.
/// </summary>
/// <param name="environment">The host environment.</param>
private static ModuleOptions<CoreDbContext> GetModuleOptions(IHostEnvironment environment) =>
    new()
    {
        ModuleName = CoreConstants.ModuleName,
        SchemaName = CoreConstants.SchemaName,
        EnableMigrations = !environment.IsEnvironment("Testing"),
        EnableSeeding = false,
    };
```

Note the one behavioural change in that block: Core stops running migrations at boot in
the Testing environment. That is safe because `PostgresFixture.ApplyMigrationsAsync`
(`tests/Integration/Common/Fixtures/PostgresFixture.cs:88-117`) already migrates all
four contexts before the host is built, and it makes Core consistent with the other
three. Verify it against a fresh container rather than assuming it.

`Program.cs:74-80` passes the environment through:

```csharp
// src/Api/Program.cs:74-80 — after
builder
    .Services.AddIdentityModule(builder.Environment)
    .AddCoreModule(builder.Environment)
    .AddContentModule(builder.Environment)
    .AddMailerModule(builder.Environment)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c => c.AddSwaggerOptions());
```

**If this is done wrong** — if `clobberExistingVars: false` is applied but a module
keeps reading the raw variable — local runs continue to seed at boot while CI does not,
and the two behaviours stay indistinguishable from a test failure.

### 3. Restore pooled contexts with explicit interceptors

File: `tests/Integration/Common/Fixtures/ApiFixture.cs`.

**Land [02-test-isolation.md](02-test-isolation.md) Change 2 first.** See Prerequisites.

Production pools every module context: `ModuleOptions<TDbContext>.UseConnectionPooling`
defaults to `true` (`src/Shared/Shared/Infrastructure/ModuleOptions.cs:38`), no module
overrides it, and `BaseModule.AddModuleDatabase` therefore takes the
`AddDbContextPool` branch (`src/Shared/Shared/Infrastructure/BaseModule.cs:47-64`) and
attaches interceptors explicitly in `ConfigureDbContextOptions`
(`BaseModule.cs:158-163`). The fixture replaces that with `AddDbContext` and no
interceptor call.

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:110-131 — before
private void ReplaceDbContext<TDbContext>(IServiceCollection services)
    where TDbContext : DbContext
{
    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));

    if (descriptor is not null)
    {
        services.Remove(descriptor);
    }

    var poolDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TDbContext));

    if (poolDescriptor is not null)
    {
        services.Remove(poolDescriptor);
    }

    services.AddDbContext<TDbContext>(options =>
    {
        options.UseNpgsql(_db.ConnectionString).UseSnakeCaseNamingConvention();
    });
}
```

```csharp
// after
/// <summary>
/// Points a module DbContext at the Testcontainer database while keeping the registration
/// production uses. Every descriptor EF Core adds for a pooled context is removed first,
/// because <c>AddDbContextPool</c> registers the pool, the lease and the options with
/// <c>TryAdd</c>: a surviving descriptor would silently win over the one registered here.
/// The interceptors are attached by hand for the same reason <c>BaseModule</c> attaches
/// them by hand — <c>BaseApiTest.SaveSeededAsync</c> depends on the dispatch interceptor
/// being present, and an implicit resolution rule is not something to hang that on.
/// </summary>
/// <typeparam name="TDbContext">The module context to re-register.</typeparam>
/// <param name="services">The test host's service collection.</param>
private void ReplaceDbContext<TDbContext>(IServiceCollection services)
    where TDbContext : DbContext
{
    Type[] pooledDescriptorTypes =
    [
        typeof(DbContextOptions<TDbContext>),
        typeof(TDbContext),
        typeof(IDbContextPool<TDbContext>),
        typeof(IScopedDbContextLease<TDbContext>),
    ];

    List<ServiceDescriptor> existing = services
        .Where(descriptor => pooledDescriptorTypes.Contains(descriptor.ServiceType))
        .ToList();

    foreach (ServiceDescriptor descriptor in existing)
    {
        services.Remove(descriptor);
    }

    services.AddDbContextPool<TDbContext>(
        (serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(_db.ConnectionString).UseSnakeCaseNamingConvention();
        }
    );
}
```

Add `using Microsoft.EntityFrameworkCore.Diagnostics;` for `ISaveChangesInterceptor` and
`using Microsoft.EntityFrameworkCore.Internal;` for `IDbContextPool<>` and
`IScopedDbContextLease<>`.

`IDbContextPool<TDbContext>` and `IScopedDbContextLease<TDbContext>` are registered by
the production `AddDbContextPool` call and are orphaned by the current replacement —
nothing resolves them once `TDbContext` comes from `AddDbContext`. They must be removed
rather than left, because every EF Core registration in `AddDbContextPool` is a
`TryAdd`: leaving them means the fixture's own call is a no-op for those two services
and the wiring depends on a descriptor the fixture did not write.

**If this is done wrong** — if the descriptors are not removed, or if the interceptor
line is dropped — either the context resolves through a pool built from options the
fixture did not configure, or the dispatch interceptor detaches and
`BaseApiTest.SaveSeededAsync` (`tests/Integration/Common/Base/BaseApiTest.cs:99-109`)
becomes a no-op that still passes, letting production event handlers fire against every
test's arrangement.

### 4. Delete the JWT override and prove the issued token works

Files: `tests/Integration/Common/Fixtures/ApiFixture.cs`,
`tests/Integration/Workflows/AuthenticationFlowTests.cs`,
`tests/Integration/Common/Extensions/HttpClientExtensions.cs`.

Requires Change 2.

a. Delete `OverrideJwtAuthentication` (`ApiFixture.cs:133-156`) and its call site. The
   method exists because `.env` clobbered `JWT_SECRET`; its own doc comment says so.
   Once Change 2 lands, the values the fixture sets at `ApiFixture.cs:77-79` —
   `ThisIsAVerySecureSecretKeyForTesting123!@#`, `116_test`, `116_test_client` — reach
   `AppEnvironment.Jwt()` and are read by `IdentityModule.cs:200-215` when the host is
   built. They are byte-identical to `Jwt.ValidSecret`, `Jwt.ValidIssuer` and
   `Jwt.ValidAudience` in `tests/Fixtures/Constants/Identity/TestConstants.Jwt.cs:11-13`,
   so the hand-minted tokens in `HttpClientExtensions` keep validating against the
   production parameters. Confirm that equality before deleting anything; if the two ever
   diverge, the fixture's environment values are what change, never the override.

b. Add the round trip the suite has never had. `AuthenticationFlowTests.cs:16-41` is
   named `SignUpAndLogin_ShouldGrantAccessToProtectedEndpoints` and does neither: it
   signs up, asserts the token has three dot-separated segments, and asserts a user row
   exists.

```csharp
// tests/Integration/Workflows/AuthenticationFlowTests.cs — new test
/// <summary>
/// Signs up, logs in, and calls a protected endpoint with the access token the application
/// issued. This is the only test where the credential under test is the one
/// <c>JwtService</c> produced, so it is the only guard on the claim contract: the subject,
/// role and session claims, the issuer, the audience and the signing key all have to line
/// up between issuance and validation for the final assertion to hold.
/// </summary>
[Fact]
public async Task Login_ThenCallProtectedEndpointWithTheIssuedToken_ResolvesTheCaller()
{
    await SeedAsync<IdentityDbContext>(context =>
        context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), nameof(EnumCoreUserRole.Visitor)))
    );

    Client.ClearAuthentication();
    Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

    string email = $"issued-{Guid.NewGuid():N}@test.com";
    var signupRequest = new PublicSignUpRequest(
        Email: email,
        UserName: $"u{Guid.NewGuid():N}"[..10],
        Password: TestAuth.ValidPassword
    );

    HttpResponseMessage signup = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);
    signup.StatusCode.Should().Be(HttpStatusCode.Created);

    await using (IdentityDbContext seedContext = CreateDbContext<IdentityDbContext>())
    {
        UserEntity user = await seedContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await seedContext.SaveChangesAsync();
    }

    HttpResponseMessage login = await Client.PostAsJsonAsync(
        Routes.Public.Auth.Login(),
        new PublicLoginRequest(Credentials: email, Password: TestAuth.ValidPassword)
    );
    login.StatusCode.Should().Be(HttpStatusCode.OK);

    PublicLoginMobileResponse body = await login.ReadAsAsync<PublicLoginMobileResponse>();
    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.AccessToken);

    HttpResponseMessage protectedResponse = await Client.GetAsync(Routes.Public.Me.Profile());

    protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    PublicGetOwnProfileResponse profile = await protectedResponse.ReadAsAsync<PublicGetOwnProfileResponse>();
    profile.User.Email.Should().Be(email, "the endpoint resolved the caller from the issued token's claims");
}
```

`PublicGetOwnProfileEndpointV1Tests.cs:11-23` already makes the same assertion against
`Routes.Public.Me.Profile()` with a minted token, so the only new element is the
credential.

c. Rename `SignUpAndLogin_ShouldGrantAccessToProtectedEndpoints`
   (`AuthenticationFlowTests.cs:16`) to `SignUp_PersistsTheUserAndReturnsTokens`, which
   is what it asserts, and drop the
   `signupBody.AccessToken.Split('.').Should().HaveCount(3)` line: three segments is a
   property of JWT encoding, not of this application, and the new test now covers what
   the old name promised.

d. Replace the role string literals in `HttpClientExtensions.cs:21`, `:29` and `:37`
   with `nameof(EnumCoreUserRole.SuperAdmin)`, `nameof(EnumCoreUserRole.Admin)` and
   `nameof(EnumCoreUserRole.Visitor)`. Production compares against the same `nameof`
   at
   `src/Modules/Identity/Identity/Application/Roles/Specifications/UserRoleSpecifications.cs:19`,
   so a renamed enum member currently leaves the tests passing against the old string.

**If this is done wrong** — if the override is deleted before Change 2 lands — every
authenticated test returns 401, because the host signs and validates with whatever
`.env` supplies while the test project mints with `Jwt.ValidSecret`.

### 5. Replace the mocked seeding tests with a Development host

Files: `tests/Integration/Common/Fixtures/ApiFixture.cs`, three new fixture files,
`tests/Integration/Modules/Identity/IdentityModuleSeedingTests.cs`,
`tests/Integration/Modules/Content/ContentModuleSeedingTests.cs`.

Those two tests are the only uses of Moq under `tests/Integration/`. Each mutates
`ASPNETCORE_ENVIRONMENT` in the test body, drives an extension method against a
`Mock<IApplicationBuilder>`, and asserts `result.Should().BeSameAs(applicationBuilder.Object)`
— that an extension method returned its own `this`, which cannot fail — plus
`context.Roles.AnyAsync()`, which on a machine with a `.env` is already true because
seeding ran at host boot.

First give `ApiFixture` the extension point, alongside the existing `DisableRateLimits`:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs — after
/// <summary>
/// The environment the host boots in. Defaults to <c>Testing</c>, which disables the
/// module migration and seeding branches. Derived fixtures override it to exercise the
/// startup path a non-Testing deployment takes.
/// </summary>
protected virtual string EnvironmentName => "Testing";

/// <inheritdoc />
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    SetEnvironmentVariables();

    builder.UseEnvironment(EnvironmentName);

    // ConfigureTestServices as in Change 1
}
```

`SetEnvironmentVariables` keeps setting the process variable to `Testing`. After
Change 2 nothing in `src/` reads it for module options, and leaving it alone means the
seeding host cannot race the shared host through a process-global.

```csharp
// tests/Integration/Common/Fixtures/SeedingApiFixture.cs — new
namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> that boots the application as Development, so the migration
/// and seeding branches of every module's <c>Use*Module</c> extension run at startup exactly
/// as they do outside the test environment.
/// </summary>
/// <param name="db">The Testcontainer database backing this host.</param>
public class SeedingApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <inheritdoc />
    protected override string EnvironmentName => "Development";
}
```

```csharp
// tests/Integration/Common/Fixtures/SeedingPostgresFixture.cs — new
namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the seeding tests. It runs its own
/// container and its own host, because the rows its seeders write at boot would otherwise
/// be truncated by the shared collection's per-test reset.
/// </summary>
public class SeedingPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new SeedingApiFixture(this);
}
```

```csharp
// tests/Integration/Common/Fixtures/SeedingCollection.cs — new
namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "Seeding" xUnit collection, which owns the only host that boots outside the
/// Testing environment.
/// </summary>
[CollectionDefinition("Seeding", DisableParallelization = true)]
public class SeedingCollection : ICollectionFixture<SeedingPostgresFixture>;
```

```csharp
// tests/Integration/Modules/Identity/IdentityModuleSeedingTests.cs — after
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Identity;

/// <summary>
/// Covers the seeding branch of <c>UseIdentityModule</c> the way the application reaches it:
/// a host booted as Development runs the branch at startup, so the assertion is about rows
/// the seeders own rather than about an extension method returning its own argument.
/// </summary>
/// <param name="db">The Development-environment host and its container.</param>
[Collection("Seeding")]
public class IdentityModuleSeedingTests(SeedingPostgresFixture db)
{
    [Fact]
    public async Task DevelopmentHost_RunsTheIdentitySeeders()
    {
        using IServiceScope scope = db.Api.Services.CreateScope();
        await using IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        (await context.Roles.AnyAsync(role => role.Name == nameof(EnumCoreUserRole.Visitor)))
            .Should()
            .BeTrue("VisitorRoleSeeder runs when the host boots outside the Testing environment");
    }
}
```

`ContentModuleSeedingTests` becomes the same shape against
`context.ContentTypes.AnyAsync(type => type.Name == /* the name ContentTypeSeeder writes */)`.
Read `ContentTypeSeeder` and name the row rather than calling bare `AnyAsync()`.

**If this is done wrong** — if the assertion stays `AnyAsync()` with no predicate — it
passes whenever any row exists from any source, which is the exact weakness that made
the current tests meaningless on a developer machine.

## Expected fallout

**The four module unit test classes stop compiling.** `Add*Module()` gains a parameter,
and it is called 52 times: `AddIdentityModule()` 12, `AddContentModule()` 15,
`AddMailerModule()` 15, `AddCoreModule()` 10 (counts include the four call sites in
`Program.cs`). Every test-side call needs an `IHostEnvironment`. This is mechanical and
it is the point: those tests currently reach around the module by mutating a
process-global, and after this change they state the environment as an argument.

```csharp
// tests/Unit/Modules/Content/ContentModuleTests.cs — the shape to move to
private static IHostEnvironment Environment(string name)
{
    var environment = new Mock<IHostEnvironment>();
    environment.SetupGet(host => host.EnvironmentName).Returns(name);
    return environment.Object;
}
```

`IsEnvironment` is an extension over `EnvironmentName`, so stubbing the property is
enough. The `Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", ...)` blocks
in `IdentityModuleTests.cs:184-202` and `:218-267`, `ContentModuleTests.cs:181-197`,
`:206-222` and `:237-275`, and `MailerModuleTests.cs:249-265` all become unnecessary and
should be deleted, not merely wrapped. That shrinks the `EnvironmentVariable` collection
that [02-test-isolation.md](02-test-isolation.md) Change 4 has to police.

**`UseIdentityModule` and `UseContentModule` unit tests that use a mocked
`IApplicationBuilder` will throw.** `Use*Module` now calls
`app.ApplicationServices.GetRequiredService<IHostEnvironment>()`, and the hand-built
providers in `IdentityModuleTests.cs:189-193` and `ContentModuleTests.cs:210-215` do not
register one. Register a stub `IHostEnvironment` in those service collections. Do not
weaken the production code to tolerate a missing environment.

**Some tests that pass locally will start failing, and that is the fix working.** Until
now a developer machine booted the host as `Development`: migrations ran, `SuperAdminSeeder`
and `VisitorRoleSeeder` ran, and `ContentTypeSeeder` ran, at host construction. After
Change 2 the host boots as `Testing` everywhere, so any test that silently depended on a
seeded role, a seeded content type or the super-admin row now has to seed it. Each such
failure is a test that was passing on one machine and one machine only. Fix the test's
arrangement; do not restore boot-time seeding.

**Empty-body error assertions may change shape.** With `OverrideJwtAuthentication`
removed, authentication failures come from the production `JwtBearerEvents`
configuration (`IdentityModule.cs:217`) rather than the fixture's replacement. Any
difference lands on 401 assertions, which are status-only today; note anything that
moves and carry it into [04-error-assertion-discipline.md](04-error-assertion-discipline.md).

**The `stub.Sent.Count` baselines become dead weight.** `EmailDeliveryFlowTests.cs:307`
and `:317` read a baseline count solely to tolerate background sends. With the scheduler
gone they are safe to replace with exact assertions; that edit belongs to
[02-test-isolation.md](02-test-isolation.md) Change 1, which also clears the stub between
tests.

**A third container appears.** `SeedingPostgresFixture` starts its own
`postgres:16-alpine`, joining the shared one and the rate-limiting one. Expect the suite
wall clock to grow by one container start.

## Testing

```bash
dotnet build
dotnet test tests/Unit
dotnet test tests/Integration
```

Then, because the whole point is determinism:

```bash
dotnet test tests/Integration && dotnet test tests/Integration
```

Both runs must report identical pass counts and identical test names. A test that
passes in one run and fails in the other means a mutation source survived.

Invariants that must hold afterwards:

```bash
# No Moq anywhere in the integration project.
grep -rn "Mock<" tests/Integration --include=*.cs

# No test method mutates the process environment.
grep -rn "Environment.SetEnvironmentVariable" tests/Integration --include=*.cs

# The JWT override is gone.
grep -rn "OverrideJwtAuthentication\|PostConfigure<JwtBearerOptions>" tests/Integration --include=*.cs

# No module reads the raw variable.
grep -rn "ASPNETCORE_ENVIRONMENT" src/Modules --include=*.cs
```

The first three must return nothing. The fourth must return only
`TokenDeliveryService.cs:181`, which is out of scope here.

New evidence the changes worked:

- `Login_ThenCallProtectedEndpointWithTheIssuedToken_ResolvesTheCaller` passes. Prove it
  can fail by deleting the session claim at
  `src/Modules/Identity/Identity/Infrastructure/Services/JwtService.cs:52` locally and
  confirming the test goes red while the rest of the authenticated suite stays green.
  Revert immediately.
- `IdentityModuleSeedingTests.DevelopmentHost_RunsTheIdentitySeeders` passes, and fails
  if `EnableSeeding` is forced to `false`.
- The two job-registration assertions still pass with the hosted service removed. If
  `CheckExists` returns false, the scheduler is not being populated on demand; assert
  against the configured `QuartzOptions` job definitions instead of weakening the test.

## Risks

**`ISchedulerFactory.GetScheduler` may behave differently without the hosted service.**
Quartz 3.16.1's service-collection factory populates jobs and triggers when the scheduler
is first requested, so `CheckExists` should still find every job key, but this is the one
part of Change 1 that has to be observed rather than reasoned about. Mitigation: run the
two registration tests first, before touching anything else; if they fail, switch them to
assert over `IOptions<QuartzOptions>.Value.JobDetails`, which is what the module actually
configured.

**Restoring pooling without scope disposal exhausts the pool.** Mitigation: the
Prerequisites section, and a build gate — do not open the pooling change until spec 02
Change 2 is on `develop`.

**The `Add*Module` signature change is broad.** 52 call sites, most of them in tests.
Mitigation: it is a compile error, not a runtime surprise, so nothing can be missed. If
the churn is genuinely unacceptable, the fallback is to keep `Add*Module(IServiceCollection)`
and have only `Use*Module` take the environment, since `AddModuleDatabase` reads neither
`EnableMigrations` nor `EnableSeeding` (`BaseModule.cs:34-66`). Record the choice here if
that fallback is taken, because it leaves two ways to obtain module options.

**Local behaviour changes for developers.** Running the API locally still reads `.env`;
only variables already exported in the shell now win. Anyone relying on `.env` to
override an exported variable will see the opposite precedence. Mitigation: call it out
in the PR description — this is standard twelve-factor precedence and matches what CI
already does.

**Seeding tests add container startup time.** Mitigation: accept it for now; spec 11
revisits container count with measurements.

## Implementation notes

Implemented 2026-08-22. Five corrections to this spec, all found by hitting the real
code:

1. **The `.env` snippet does not compile.** DotNetEnv 3.1.1's `Env.Load` has no
   `options` parameter, and `Env.TraversePath()` returns a `LoadOptions` whose own
   `Load` takes only a path. The landed form is the package's documented fluent API:
   `Env.NoClobber().Load();` and `Env.NoClobber().TraversePath().Load();`.
2. **`QuartzHostedService` is public**, so the removal matches on `typeof(...)` rather
   than the spec's string comparison on `ImplementationType?.Name`. A rename is now a
   compile error instead of a silent no-op that would restore the live scheduler.
   `AddScheduledJob` registers the hosted service per call without `TryAddEnumerable`,
   so there are four descriptors, not one; the loop removes all of them.
3. **The 401/403 fallout warning was wrong.** `OverrideJwtAuthentication` used
   `PostConfigure<JwtBearerOptions>` and assigned only `TokenValidationParameters`. It
   never touched `options.Events`, so `ConfigureJwtBearerEvents`'
   `OnChallenge`/`OnForbidden` handlers were already the only ones running in the test
   host. No 401 or 403 assertion moved, and none did.
4. **The proposed falsification does not falsify.** Deleting the session claim from
   `JwtService` leaves `Login_ThenCallProtectedEndpointWithTheIssuedToken_ResolvesTheCaller`
   green — nothing on that path validates it. The mutation that does discriminate is
   dropping `BuildRoleClaims`: the new test goes red and 521 other authenticated tests
   stay green, because they mint their own tokens. Use that mutation, not the session
   claim, if the guard is ever re-verified.
5. **The spec missed two breaking unit tests.** It names `UseIdentityModule` and
   `UseContentModule`; `MailerModuleTests` and `CoreModuleTests` break too, the latter
   twice over, because Core's `EnableMigrations` also flipped.

`EF1001` is suppressed in `tests/Integration/_116.Integration.Tests.csproj`: the
`IDbContextPool<>` and `IScopedDbContextLease<>` references in Change 3 are deliberate
internal-API usage, and the analyzer warns on them by default.

## Checklist

- [x] 1 — `DisableScheduledJobs` added to `ApiFixture`, hosted service removed, both job
      registration assertions still green
- [x] 2 — `clobberExistingVars: false` on both `Env` load calls; all four
      `GetModuleOptions` take `IHostEnvironment`; `Program.cs` passes `builder.Environment`;
      the "tests use InMemory database" comment deleted
- [x] 3 — `ReplaceDbContext` uses `AddDbContextPool` with explicit `AddInterceptors`, and
      removes the options, context, pool and lease descriptors (landed after spec 02
      Change 2)
- [x] 4 — `OverrideJwtAuthentication` deleted; the login round-trip test added; the
      mis-named signup test renamed; `HttpClientExtensions` roles use `nameof`
- [x] 5 — `EnvironmentName` override added; `SeedingApiFixture`, `SeedingPostgresFixture`
      and `SeedingCollection` added; both seeding tests rewritten to assert the specific
      rows their seeders own; Moq removed from `tests/Integration/`
- [x] Module unit tests take a stubbed `IHostEnvironment` and no longer mutate
      `ASPNETCORE_ENVIRONMENT`
- [ ] Full integration suite run twice back to back with identical results
- [ ] A CI run and a local run produce the same result on the full suite
