# High — The test host diverges from production wiring

`ApiFixture` boots the real application and replaces its outbound edges, which is
the right design. But three of its substitutions go past the edge and change how
the application is composed: DbContexts lose their pooling, interceptors are
re-attached only implicitly, and two tests drive a startup branch through a mocked
`IApplicationBuilder` after mutating the process environment. Each is judged
separately below, because they are not equally serious and treating them as one
finding would overstate two of them.

## The problem

### (a) Pooled contexts become unpooled

Production pools every module context. `ModuleOptions<TDbContext>.UseConnectionPooling`
defaults to `true` (`src/Shared/Shared/Infrastructure/ModuleOptions.cs:38`) and no
module overrides it, so all four contexts take the pooled branch:

```csharp
// src/Shared/Shared/Infrastructure/BaseModule.cs:47-64
if (options.UseConnectionPooling)
{
    services.AddDbContextPool<TDbContext>(
        (serviceProvider, dbOptions) =>
        {
            ConfigureDbContextOptions(serviceProvider, dbOptions, connectionString);
        }
    );
}
else
{
    services.AddDbContext<TDbContext>(...);
}
```

The fixture removes both the options registration and the context registration, and
puts back an unpooled one:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:120-130
var poolDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TDbContext));

if (poolDescriptor is not null)
{
    services.Remove(poolDescriptor);
}

services.AddDbContext<TDbContext>(options =>
{
    options.UseNpgsql(_db.ConnectionString).UseSnakeCaseNamingConvention();
});
```

Applied to `IdentityDbContext`, `CoreDbContext`, `ContentDbContext` and
`MailerDbContext` (`ApiFixture.cs:100-103`).

**Judgement: a real gap, low frequency, hard to hit any other way.** Pooled
contexts are reset and reused rather than constructed per scope. Defects specific
to that lifetime — state set in a context constructor or `OnConfiguring` that does
not survive the pool reset, a field on a derived context that is never cleared, a
`ChangeTracker` configuration applied per instance — are invisible in the test host
by construction, and they surface in production as intermittent cross-request data
bleed, which is the worst possible category to find in the wild. Nothing else in the
suite can catch them, because pooling is a property of the registration, not of the
code under test.

### (b) Interceptors are attached implicitly rather than explicitly

Production attaches them by hand while building the options:

```csharp
// src/Shared/Shared/Infrastructure/BaseModule.cs:158-163
private static void ConfigureDbContextOptions(
    IServiceProvider serviceProvider,
    DbContextOptionsBuilder options,
    string connectionString
)
{
    // Add interceptors
    options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
```

The fixture's replacement configuration action does not call `AddInterceptors`.
Verified: the string `Interceptor` does not appear anywhere in `ApiFixture.cs`.

They are nevertheless attached, because `RegisterInterceptorsIfNotExists`
(`BaseModule.cs:116-145`) registers both `AuditableEntityInterceptor` and
`DispatchDomainEventsInterceptor` as `ISaveChangesInterceptor` singletons in the
application container, and EF Core resolves `IInterceptor` registrations from the
application service provider on its own. Two integration tests prove it holds in
this host:

```csharp
// tests/Integration/Shared/Infrastructure/Interceptors/DispatchDomainEventsInterceptorTests.cs:36
role.DomainEvents.Should().BeEmpty("the interceptor clears domain events once they are dispatched on save");

// tests/Integration/Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs:32
saved.CreatedBy.Should().Be(nameof(EnumAuditActor.System), "non-HTTP saves are attributed to the system actor");
```

**Judgement: currently correct, but load-bearing and undeclared.** The dependency
runs deeper than those two tests. `BaseApiTest.SaveSeededAsync` exists *because* the
dispatch interceptor is attached to fixture-created contexts, and its doc comment
states the reason outright:

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs:90-95
/// The contexts returned by <see cref="CreateDbContext{TDbContext}" /> come from the
/// application container, so the dispatch interceptor is attached and those events would fire
/// their production handlers — welcome emails, notification rows, promotion stamps — against
/// the arrangement of every test.
```

If the interceptor silently detached, `SaveSeededAsync` would keep passing while
becoming a no-op, and every test that relies on seeding *not* firing production
handlers would still be green — for the wrong reason. Relying on an implicit
resolution rule to hold a documented invariant is the risk; the invariant itself is
fine. The fix is to make the fixture say what production says, so the two
registrations cannot drift apart.

### (c) Two tests mock `IApplicationBuilder` and mutate the process environment

```csharp
// tests/Integration/Modules/Identity/IdentityModuleSeedingTests.cs:22-39
string? previousEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

try
{
    var applicationBuilder = new Mock<IApplicationBuilder>();
    applicationBuilder.Setup(builder => builder.ApplicationServices).Returns(Api.Services);

    IApplicationBuilder result = applicationBuilder.Object.UseIdentityModule();

    result.Should().BeSameAs(applicationBuilder.Object);

    await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
    (await context.Roles.AnyAsync()).Should().BeTrue();
}
finally
{
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previousEnvironment);
}
```

`tests/Integration/Modules/Content/ContentModuleSeedingTests.cs:22-39` is the same
test with `UseContentModule` and `ContentTypes` substituted. These are the **only**
two uses of Moq in the entire integration project — `grep -rn "Mock<" tests/Integration`
returns four lines, all of them these two files' `using Moq;` and `new Mock<IApplicationBuilder>()`.

**Judgement: the clearest violation of the three.** Three separate problems stack:

1. A mocked `IApplicationBuilder` is not a real entry point. The project's own rule
   permits exactly two — real HTTP through `BaseApiTest`, or a real repository from
   DI through `BaseRepositoryTest`. This is neither, and it sits in
   `tests/Integration/` where the coverage signal is supposed to mean "the
   application wires this up."
2. The assertion `result.Should().BeSameAs(applicationBuilder.Object)` asserts that
   an extension method returned its own `this`. It cannot fail.
3. The environment mutation is restored in a `finally`, so the blast radius is
   bounded, but it makes the correctness of a global process variable depend on a
   test's exception handling — in a suite where 356 classes share one process.

Both tests also encode the assumption that the integration host runs under
`Testing`, which [02-environment-divergence.md](02-environment-divergence.md) shows
is true on CI and false on any machine with a `.env`. Locally, seeding has already
run at host boot, so `context.Roles.AnyAsync()` would be true whether
`UseIdentityModule` did anything or not. The test proves nothing on a developer
machine and proves the wrong thing on CI.

## Why it matters

Coverage from `tests/Integration/` carries a specific meaning in this codebase: it
says the application actually reaches the code. `IdentityModuleSeedingTests` turns
that signal green for a startup branch the real host never executes, using a mock to
get there. That is precisely the failure the repository's testing rules name — the
metric that was warning you gets satisfied without the wiring being fixed.

The pooling divergence is quieter but the same category. `AddDbContextPool` is a
production decision with production consequences, and the suite has arranged never
to test it. When the bug arrives it will be reported as flaky data, not as a
pooling defect, and there will be no test that could have caught it.

## The fix

**Keep the pooling decision the host made.** The fixture only needs to change the
connection string, not the registration strategy:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs — before
services.AddDbContext<TDbContext>(options =>
{
    options.UseNpgsql(_db.ConnectionString).UseSnakeCaseNamingConvention();
});

// after — same lifetime as production, same interceptor attachment as production
services.AddDbContextPool<TDbContext>(
    (serviceProvider, options) =>
    {
        options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
        options.UseNpgsql(_db.ConnectionString).UseSnakeCaseNamingConvention();
    }
);
```

This closes (a) and (b) in one edit. Pooled contexts are safe here: `CreateDbContext`
returns them from a scope like any other consumer, and pooled contexts are returned
to the pool when the scope is disposed — which is the change
[07-lifecycle-and-scope-leaks.md](07-lifecycle-and-scope-leaks.md) asks for anyway.
Sequence the two together.

**Replace the seeding tests with a host that boots as `Development`.** Once
[02](02-environment-divergence.md) makes `IHostEnvironment` authoritative, the
branch can be exercised the way the application exercises it:

```csharp
// tests/Integration/Common/Fixtures/SeedingApiFixture.cs
/// <summary>
/// An <see cref="ApiFixture" /> that boots the application as Development, so the
/// migration and seeding branches of every module's <c>Use*Module</c> extension run
/// at startup exactly as they do outside the test environment.
/// </summary>
/// <param name="db">The Testcontainer database backing this host.</param>
public class SeedingApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <inheritdoc />
    protected override string EnvironmentName => "Development";
}

// tests/Integration/Common/Fixtures/SeedingPostgresFixture.cs
public class SeedingPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new SeedingApiFixture(this);
}

// tests/Integration/Modules/Identity/IdentityModuleSeedingTests.cs — after
[Collection("Seeding")]
public class IdentityModuleSeedingTests(SeedingPostgresFixture db)
{
    [Fact]
    public async Task DevelopmentHost_RunsIdentitySeeders()
    {
        await using IdentityDbContext context = db.Api.Services
            .CreateScope()
            .ServiceProvider.GetRequiredService<IdentityDbContext>();

        (await context.Roles.AnyAsync(r => r.Name == nameof(EnumCoreUserRole.Visitor)))
            .Should()
            .BeTrue("VisitorRoleSeeder runs when the host boots outside the Testing environment");
    }
}
```

`ApiFixture` already has the extension point this needs — `RateLimitedApiFixture`
uses the same `protected virtual` override pattern to change one host decision and
inherit the rest. Adding an `EnvironmentName` override alongside `DisableRateLimits`
follows the fixture's existing design rather than inventing a second one.

Note the stronger assertion: `AnyAsync(r => r.Name == ...)` names the row the seeder
is responsible for. `AnyAsync()` on the whole table passes if *any* role exists from
any source, which on a `.env`-affected machine it always does.

## The principle

**Substitute the outbound edges, keep the composition.** A DbContext pointed at a
container is a replaced edge. A DbContext registered with a different lifetime is a
different application. The line is: does the change alter what the code under test
talks to, or does it alter how the code under test is assembled? The first is
necessary; the second removes exactly the defects that only exist in the assembly.

And **a mock in an integration test is a request to move the test.** If the only way
to reach a branch is to fake the framework object that would have called it, either
the branch is unreachable in production — a defect in `src/` — or the test belongs
in `tests/Unit/`.

## What is worth protecting

The rest of the boundary discipline in this project is better than most codebases
achieve, and none of it should be traded away to fix the above. Measured across all
of `tests/Integration/`:

| Anti-pattern | Occurrences |
| --- | --- |
| `Mock<I*Repository>` or `Mock<*DbContext>` | 0 |
| Reflection into private members (`BindingFlags`, `GetField`, `GetProperty`) | 0 |
| Hand-built `new ServiceCollection()` | 0 |
| Total Moq usage | 2 files, both listed above |

Every other integration test in this suite reaches its target through real HTTP or a
real repository from the container. That is the property that makes integration
coverage meaningful here, and it is why the two seeding tests stand out enough to be
worth a document.

## Checklist

- [ ] `ApiFixture.ReplaceDbContext` uses `AddDbContextPool` with an explicit
      `AddInterceptors` call, matching `BaseModule.ConfigureDbContextOptions`
- [ ] Scopes are disposed ([07](07-lifecycle-and-scope-leaks.md)) before pooling is
      restored, so pooled contexts are actually returned to the pool
- [ ] `ApiFixture` exposes a `protected virtual string EnvironmentName` used by
      `builder.UseEnvironment(...)`
- [ ] `IdentityModuleSeedingTests` and `ContentModuleSeedingTests` boot a
      Development host and assert the specific rows their seeders own
- [ ] `Moq` is no longer referenced anywhere under `tests/Integration/`
- [ ] No `Environment.SetEnvironmentVariable` call remains in a test method
