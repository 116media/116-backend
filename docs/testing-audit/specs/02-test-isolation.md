# Spec 02 — Test isolation

## Goal

Isolation in this suite is enforced in some places and left to author discipline in
others, and every place it is left to discipline has leaked. Singleton stubs carry
one-shot failure flags and unbounded accumulators that no reset clears; three helper
methods open roughly 1,189 DI scopes per run and drop the references; 104 unit test
files set the ambient culture and never restore it; eight classes mutate process-global
environment variables outside the collection that exists to serialise exactly that; and
73 `Faker` instances draw from one shared, non-thread-safe `Random`. This spec moves
every one of those obligations into the harness, where it cannot be forgotten. It is
second because it is what makes a red test attributable: until state stops flowing from
one test into the next, a failure names the test that observed the damage rather than
the test that caused it.

Backing findings: [../integration/05-shared-mutable-state.md](../integration/05-shared-mutable-state.md),
[../integration/07-lifecycle-and-scope-leaks.md](../integration/07-lifecycle-and-scope-leaks.md),
[../unit/03-culture-and-environment-leakage.md](../unit/03-culture-and-environment-leakage.md),
[../fixtures/04-random-data-determinism.md](../fixtures/04-random-data-determinism.md).

## Scope

In this spec:

- An `IResettableStub` contract, implemented by the three external-service stubs,
  registered under the interface in `ApiFixture`, and driven from both integration base
  classes' `InitializeAsync`.
- `StubStreamingLinkResolutionService` converted from static state to instance state,
  registered as a singleton, with its hand-written `Reset()` and the 13 calls to it
  deleted.
- Scope tracking and disposal in `BaseApiTest` and `BaseRepositoryTest`.
- `CultureScope` extended to save and restore `CurrentCulture` as well as
  `CurrentUICulture`, and substituted at all 104 sites.
- The eight environment-mutating unit test classes joined to the `EnvironmentVariable`
  collection, with a restoring `Dispose` where one is missing.
- `TestFaker.Create()` and the 73 `Faker` declarations that call it.

Not in this spec:

- Removing the Quartz scheduler, which is the other consumer of the stubs' state —
  [01-test-host-fidelity.md](01-test-host-fidelity.md) Change 1.
- Restoring `AddDbContextPool`. Change 2 here unblocks it; the change itself is
  [01-test-host-fidelity.md](01-test-host-fidelity.md) Change 3.
- Replacing the 104 localization tests with a resource-completeness theory. This spec
  makes those tests stop leaking; whether they should exist at all is spec 06.
- Introducing `JwtOptions` so that `src/` stops reading the environment at the point of
  use. That is the durable fix behind Change 4 and it is spec 09's territory; Change 4
  makes the current design safe, not good.
- Sharding collections or widening parallelism — spec 11.

## Prerequisites

None. Every change here is independent of the other specs, and Change 2 is a
prerequisite for [01-test-host-fidelity.md](01-test-host-fidelity.md) Change 3.

Sequence Changes 1 and 2 before 3, 4 and 5 if the work is split across people: the first
two touch four files between them and remove whole categories of cross-test coupling,
which makes the wide mechanical edits in 3 and 5 safe to evaluate.

## Changes

### 1. Give the stubs a reset contract and drive it from the base classes

Files: `tests/Integration/Common/Stubs/IResettableStub.cs` (new),
`tests/Integration/Common/Stubs/StubEmailSender.cs`,
`tests/Integration/Common/Stubs/StubCloudinaryService.cs`,
`tests/Integration/Common/Stubs/StubStreamingLinkResolutionService.cs`,
`tests/Integration/Common/Fixtures/ApiFixture.cs`,
`tests/Integration/Common/Base/BaseApiTest.cs`,
`tests/Integration/Common/Base/BaseRepositoryTest.cs`, and the two streaming-link
endpoint test files.

```csharp
// tests/Integration/Common/Stubs/IResettableStub.cs — new
namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Implemented by every external-service stub that carries state across requests. The
/// integration base classes clear all registered stubs before each test, so a queued
/// one-shot failure or a recorded call can never be observed by the next test.
/// </summary>
public interface IResettableStub
{
    /// <summary>
    /// Returns the stub to the state it had when the host was first built.
    /// </summary>
    void Reset();
}
```

`StubEmailSender` holds `Sent` (`StubEmailSender.cs:18`) and `NextFailure`
(`:24`); `StubCloudinaryService` holds `NextDeleteFailure`
(`StubCloudinaryService.cs:19`) and `DeletedPublicIds` (`:28`). Both implement the
interface:

```csharp
// tests/Integration/Common/Stubs/StubEmailSender.cs — after
public class StubEmailSender : IEmailSender, IResettableStub
{
    // Sent, NextFailure and SendAsync unchanged.

    /// <inheritdoc />
    public void Reset()
    {
        Sent.Clear();
        NextFailure = null;
    }
}
```

```csharp
// tests/Integration/Common/Stubs/StubCloudinaryService.cs — after
public class StubCloudinaryService : ICloudinaryService, IResettableStub
{
    // NextDeleteFailure, DeletedPublicIds and the upload/delete members unchanged.

    /// <inheritdoc />
    public void Reset()
    {
        NextDeleteFailure = null;
        DeletedPublicIds.Clear();
    }
}
```

`StubStreamingLinkResolutionService` needs more: its two hooks are `static`
(`StubStreamingLinkResolutionService.cs:18` and `:24`), which makes them shared across
every host in the process, including the rate-limited one and the seeding one.

```csharp
// tests/Integration/Common/Stubs/StubStreamingLinkResolutionService.cs — after
/// <summary>
/// In-memory stub replacing the Odesli-backed resolution service so integration tests never
/// call the real provider. Behaviour is scripted per test through the instance hooks below;
/// the base classes clear them before each test, so no test needs to reset anything itself.
/// </summary>
public class StubStreamingLinkResolutionService : IStreamingLinkResolutionService, IResettableStub
{
    /// <summary>
    /// The platform links the next resolutions return. Defaults to every modelled platform
    /// so the happy path needs no arrangement.
    /// </summary>
    public IReadOnlyDictionary<EnumStreamingPlatform, string> NextResult { get; set; } = DefaultResult();

    /// <summary>
    /// When set, the next resolutions throw this instead of returning
    /// <see cref="NextResult" />.
    /// </summary>
    public StreamingLinkResolutionException? NextException { get; set; }

    /// <inheritdoc />
    public void Reset()
    {
        NextResult = DefaultResult();
        NextException = null;
    }

    // ResolveAsync and DefaultResult unchanged, except that ResolveAsync now reads the
    // instance properties.
}
```

Registration moves from the scoped `Replace<,>` helper (`ApiFixture.cs:213`) to the
singleton pattern the other two stubs already use, and all three are additionally
registered under the interface:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs — after
private static void ReplaceCloudinaryService(IServiceCollection services)
{
    RemoveAll<ICloudinaryService>(services);

    services.AddSingleton<StubCloudinaryService>();
    services.AddSingleton<ICloudinaryService>(sp => sp.GetRequiredService<StubCloudinaryService>());
    services.AddSingleton<IResettableStub>(sp => sp.GetRequiredService<StubCloudinaryService>());
}
```

with the same three-line shape for `StubEmailSender` and
`StubStreamingLinkResolutionService`. `Replace<TService, TImpl>` (`ApiFixture.cs:254-265`)
stays for `IYoutubeThumbnailService`, which holds no state.

Then the base classes drive it:

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs:117-126 — before
public async ValueTask InitializeAsync()
{
    await Db.ResetAsync();
    InvalidateTagCache();
    InvalidatePopularArticlesCache();
    InvalidatePopularVideosCache();
    await SeedTestUsersAsync();
    await SeedAsync();
}
```

```csharp
// after
public async ValueTask InitializeAsync()
{
    await Db.ResetAsync();
    ResetStubs();
    InvalidateTagCache();
    InvalidatePopularArticlesCache();
    InvalidatePopularVideosCache();
    await SeedTestUsersAsync();
    await SeedAsync();
}

/// <summary>
/// Clears every external-service stub before each test. The stubs are singletons in the
/// shared <see cref="ApiFixture" />, so queued one-shot failures and recorded calls outlive
/// <see cref="PostgresFixture.ResetAsync" /> exactly as the in-memory caches do. Any stub
/// added later is covered the moment it implements <see cref="IResettableStub" />.
/// </summary>
private void ResetStubs()
{
    using IServiceScope scope = Api.Services.CreateScope();

    foreach (IResettableStub stub in scope.ServiceProvider.GetServices<IResettableStub>())
    {
        stub.Reset();
    }
}
```

`BaseRepositoryTest.InitializeAsync` (`BaseRepositoryTest.cs:67-70`) gets the identical
call and the identical private method.

Finally, delete what the leak forced. The 13 `StubStreamingLinkResolutionService.Reset()`
calls at `AdminResolveSingleStreamingLinksEndpointV1Tests.cs:42, 83, 111, 129` and
`AdminResolveAlbumStreamingLinksEndpointV1Tests.cs:39, 53, 71, 99, 131, 170, 190, 209, 227`
go away entirely. The eight sites that script the hooks —
`AdminResolveSingleStreamingLinksEndpointV1Tests.cs:43, 130` and
`AdminResolveAlbumStreamingLinksEndpointV1Tests.cs:105, 145, 171, 191, 210` plus the
doc-comment reference at `:15` — must resolve the instance instead of writing a static:

```csharp
// tests/Integration/.../AdminResolveSingleStreamingLinksEndpointV1Tests.cs — before
StubStreamingLinkResolutionService.Reset();
StubStreamingLinkResolutionService.NextResult = new Dictionary<EnumStreamingPlatform, string>
{
    [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/track/1",
    [EnumStreamingPlatform.Deezer] = "https://www.deezer.com/track/5",
};
```

```csharp
// after
StreamingStub.NextResult = new Dictionary<EnumStreamingPlatform, string>
{
    [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/track/1",
    [EnumStreamingPlatform.Deezer] = "https://www.deezer.com/track/5",
};
```

with one accessor per file, matching how `AbandonedDraftCleanupJobTests.cs:27` already
reaches the Cloudinary stub:

```csharp
private StubStreamingLinkResolutionService StreamingStub =>
    Api.Services.GetRequiredService<StubStreamingLinkResolutionService>();
```

**If this is done wrong** — if the streaming stub keeps a scoped registration while
losing its static state — each request resolves a fresh instance, the scripted hook
never reaches the code under test, and the four "provider down" tests silently start
asserting the happy path.

### 2. Track and dispose the DI scopes

Files: `tests/Integration/Common/Base/BaseApiTest.cs`,
`tests/Integration/Common/Base/BaseRepositoryTest.cs`. No call site changes.

Three helpers open a scope and drop the reference: `BaseApiTest.CreateDbContext`
(`BaseApiTest.cs:49-54`), and `BaseRepositoryTest.CreateDbContext`, `Resolve` and
`CreateScopedRepository` (`BaseRepositoryTest.cs:34-64`). Measured call sites outside
`tests/Integration/Common/Base/`: 838 `CreateDbContext<`, 287 `Resolve<`, 64
`CreateScopedRepository<` — **1,189 scopes per run**, none disposed, all rooted in a
provider that lives for the whole session.

```csharp
// tests/Integration/Common/Base/BaseRepositoryTest.cs — after
[Collection("Database")]
public abstract class BaseRepositoryTest : IAsyncLifetime
{
    /// <summary>
    /// Every scope opened by <see cref="CreateDbContext{TDbContext}" />,
    /// <see cref="Resolve{TService}" /> and
    /// <see cref="CreateScopedRepository{TRepository, TDbContext}" />. The application
    /// container outlives the whole run, so a scope that is not disposed here is never
    /// disposed at all and holds its Npgsql connection until the process exits.
    /// </summary>
    private readonly List<IServiceScope> _scopes = [];

    // Postgres, Api and the constructor unchanged.

    /// <summary>
    /// Opens a scope, records it for disposal at the end of the test, and returns it.
    /// </summary>
    /// <returns>The scope, already tracked.</returns>
    private IServiceScope OpenScope()
    {
        IServiceScope scope = Api.Services.CreateScope();
        _scopes.Add(scope);
        return scope;
    }

    /// <summary>
    /// Creates a new <typeparamref name="TDbContext" /> scoped to the Testcontainer database.
    /// </summary>
    /// <typeparam name="TDbContext">The module context to resolve.</typeparam>
    /// <returns>The resolved context.</returns>
    protected TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext => OpenScope().ServiceProvider.GetRequiredService<TDbContext>();

    /// <summary>
    /// Resolves a service from the DI container via a new scope.
    /// </summary>
    /// <typeparam name="TService">The service to resolve.</typeparam>
    /// <returns>The resolved service.</returns>
    protected TService Resolve<TService>()
        where TService : notnull => OpenScope().ServiceProvider.GetRequiredService<TService>();

    /// <summary>
    /// Creates a new DI scope and returns a tuple of (repository, dbContext) sharing that
    /// scope, so that <c>SaveChangesAsync</c> persists changes made by the repository.
    /// </summary>
    /// <typeparam name="TRepository">The repository to resolve.</typeparam>
    /// <typeparam name="TDbContext">The module context to resolve.</typeparam>
    /// <returns>The repository and the context that back the same scope.</returns>
    protected (TRepository Repo, TDbContext Db) CreateScopedRepository<TRepository, TDbContext>()
        where TRepository : notnull
        where TDbContext : DbContext
    {
        IServiceProvider provider = OpenScope().ServiceProvider;
        return (provider.GetRequiredService<TRepository>(), provider.GetRequiredService<TDbContext>());
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (IServiceScope scope in _scopes)
        {
            if (scope is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                scope.Dispose();
            }
        }

        _scopes.Clear();
    }
}
```

`BaseApiTest` takes the same `_scopes` field and `OpenScope` helper, routes
`CreateDbContext` through it, and its existing `DisposeAsync` (`BaseApiTest.cs:170-175`)
gains the same loop alongside `Client.Dispose()`.

Leave the three sites that are already correct — `BaseApiTest.cs:139`, `:152` and `:165`
— as `using var scope`, because they dispose within the method.

Three properties make this safe: a plain `List<T>` is enough because both base classes
are per-test-instance and every class runs inside a serialised collection; disposing a
scope whose `DbContext` was already disposed by an `await using` at the call site is a
no-op; and no call site changes, so all 1,189 benefit from an edit to two files.

**If this is done wrong** — if the scopes are disposed in `InitializeAsync` rather than
`DisposeAsync`, or if a scope is disposed while a test still holds the context it
produced — tests fail with `ObjectDisposedException` on a `DbContext`, which is a
loud failure and preferable to the silent leak, but it means the list is being cleared
at the wrong point in the lifecycle.

### 3. Extend `CultureScope` and use it everywhere

Files: `tests/Fixtures/Helpers/CultureScope.cs`, plus 104 unit test files.

`CultureScope` (`tests/Fixtures/Helpers/CultureScope.cs:8-29`) saves and restores
`CurrentUICulture` only, while the 104 leaking sites set both cultures. Fix the helper
first, or the substitution silently stops restoring number and date formatting.

```csharp
// tests/Fixtures/Helpers/CultureScope.cs — after
using System.Globalization;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Sets the formatting culture and the resource-lookup culture for the duration of a test
/// and restores both on dispose. Tests run on pooled threads that are reused across
/// collections, so a culture left behind changes the meaning of whatever test runs next.
/// </summary>
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previousCulture;
    private readonly CultureInfo _previousUiCulture;

    /// <summary>
    /// Initializes a new instance, setting both cultures to the specified culture name.
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

Then the substitution, which is mechanical across all 104 files:

```csharp
// before — e.g. tests/Unit/.../Login/AdminLoginValidatorTests.cs:216-217
Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
```

```csharp
// after
using var cultureScope = new CultureScope(culture);
```

The `using` declaration scopes to the end of the test method, so the culture is restored
on the normal path and the exception path alike. Name the variable rather than using a
discard: `using var _ = ...` is legal but reads as an accident, and the name makes the
restoration visible to a reviewer. Delete the now-unused `using System.Globalization;`
where nothing else in the file needs `CultureInfo`.

Do this file by file rather than with a blind regex: a handful of the 104 set the
culture in a constructor or a helper rather than in the test body, and those need the
scope to become a field disposed by the class, not a local.

**If this is done wrong** — if the assignment is replaced but the helper still restores
only `CurrentUICulture` — resource lookups become deterministic while
`ToString()`-based assertions keep failing intermittently, and the remaining flakiness
looks unrelated to culture.

### 4. Put the environment mutators in the serialising collection

Files: eight unit test classes.

`EnvironmentVariableCollection` (`tests/Unit/Common/EnvironmentVariableCollection.cs:10`)
already exists with `DisableParallelization = true`, and five classes join it. These
eight mutate process-global variables from outside it:

| Class | Variables | Restores today? | Current collection |
| --- | --- | --- | --- |
| `Infrastructure/Services/TokenDeliveryServiceTests.cs` | `ASPNETCORE_ENVIRONMENT` (9 sites) | Yes, `Dispose` at `:34` | none |
| `Infrastructure/Services/JwtServiceTests.cs` | `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_ACCESS_TOKEN_EXPIRATION` (16 sites) | Yes, `Dispose` at `:52` | none |
| `Application/Session/Factories/SessionFactoryTests.cs` | `JWT_REFRESH_TOKEN_EXPIRATION` (`:35`) | **No** | none |
| `.../RefreshToken/PublicRefreshTokenFactoryTests.cs` | `JWT_REFRESH_TOKEN_EXPIRATION` (`:31`) | **No** | none |
| `.../Seeds/SuperAdmin/SuperAdminSeederTests.cs` | `DEFAULT_USER_PASSWORD` (`:45`) | Yes, `Dispose` at `:50` | `SuperAdminSeeder` |
| `.../Seeds/SuperAdmin/SuperAdminConfigurationTests.cs` | `DEFAULT_USER_PASSWORD` (`:118-199`) | Yes, per-test `finally` | `SuperAdminSeeder` |
| `.../Seeds/SuperAdmin/SuperAdminSeedingStrategyTests.cs` | `DEFAULT_USER_PASSWORD` (`:35`) | **No** | `SuperAdminSeeder` |
| `.../Seeds/SuperAdmin/SuperAdminEntityFactoryTests.cs` | `DEFAULT_USER_PASSWORD` (`:31`) | **No** | `SuperAdminSeeder` |

The four `SuperAdmin*` classes are already in a `SuperAdminSeeder` collection defined at
`SuperAdminSeederTests.cs:19-20`. **A class can belong to exactly one xUnit collection**,
so they cannot simply gain a second attribute: move them to `EnvironmentVariable` and
delete the `SuperAdminSeederCollection` definition, which then has no members. That is
a strict improvement — `DisableParallelization` on `SuperAdminSeeder` serialised those
four against each other but not against `AppEnvironmentTests`, which is in the other
collection and mutates the process too.

```csharp
// tests/Unit/.../Seeds/SuperAdmin/SuperAdminSeedingStrategyTests.cs — before
[Collection("SuperAdminSeeder")]
public class SuperAdminSeedingStrategyTests
{
    public SuperAdminSeedingStrategyTests()
    {
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", "TestPassword123!");
        // ...
    }
}
```

```csharp
// after
[Collection("EnvironmentVariable")]
public class SuperAdminSeedingStrategyTests : IDisposable
{
    private readonly string? _originalPassword;

    public SuperAdminSeedingStrategyTests()
    {
        _originalPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", "TestPassword123!");
        // ...
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", _originalPassword);
    }
}
```

`JwtServiceTests.cs:41-55` is the local pattern to copy, including restoring `null`
rather than an empty string. Apply the same shape to `SessionFactoryTests`,
`PublicRefreshTokenFactoryTests` and `SuperAdminEntityFactoryTests`; the other four need
only the collection attribute.

If [01-test-host-fidelity.md](01-test-host-fidelity.md) Change 2 has landed,
`IdentityModuleTests`, `ContentModuleTests` and `MailerModuleTests` no longer need to
touch `ASPNETCORE_ENVIRONMENT` at all and can leave the collection. Check that before
adding anyone new to it — the goal is for the collection to shrink.

**If this is done wrong** — if a class joins the collection but still leaves a variable
set — the serialisation hides the leak from the collection's own members while
`TokenDeliveryServiceTests` and the module tests keep reading whatever was left behind.

### 5. Give every `Faker` its own randomizer

Files: `tests/Fixtures/Helpers/TestFaker.cs` (new),
`tests/Fixtures/TestDataModuleInitializer.cs`, 73 builder files, and
`tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs`.

`TestDataModuleInitializer.Initialize` assigns `Randomizer.Seed = new Random(116116)`,
which replaces the single `System.Random` that every `Faker` draws from. 73 fixture
files declare `private readonly Faker _faker = new();` (55 under `Builders/Requests`, 13
under `Builders/Entities`, 4 under `Builders/Commands`, 1 under `Builders/Helpers`), and
xUnit runs collections in parallel, so the value any one fixture receives depends on how
many draws other tests made first — and `System.Random` is not thread-safe.

```csharp
// tests/Fixtures/Helpers/TestFaker.cs — new
using Bogus;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Creates <see cref="Faker" /> instances that do not share Bogus's process-wide
/// randomizer.
/// </summary>
/// <remarks>
/// Bogus seeds every <see cref="Faker" /> from a single static <see cref="Random" /> unless
/// one is supplied. Sharing it makes the value any individual fixture receives depend on how
/// many draws other tests happened to make first, and exposes a generator with unsynchronised
/// mutable state to concurrent test collections. Each instance created here owns a stream
/// derived from a fixed base seed and a monotonic counter, so a given fixture sees the same
/// values on every run.
/// </remarks>
public static class TestFaker
{
    private const int BaseSeed = 116116;

    private static int _counter;

    /// <summary>
    /// Creates a <see cref="Faker" /> with a private, deterministically seeded randomizer.
    /// </summary>
    /// <returns>A faker that draws from its own stream.</returns>
    public static Faker Create() => new() { Random = new Randomizer(BaseSeed + Interlocked.Increment(ref _counter)) };
}
```

```csharp
// tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs:15 — before
private readonly Faker _faker = new();
```

```csharp
// after
private readonly Faker _faker = TestFaker.Create();
```

Keep `TestDataModuleInitializer` as the backstop for any `Faker` created without the
helper, and correct the reproducibility claim its current doc comment makes:

```csharp
// tests/Fixtures/TestDataModuleInitializer.cs — after
/// <summary>
/// Seeds Bogus's process-wide randomizer as a backstop for any <see cref="Bogus.Faker" />
/// created without <see cref="Helpers.TestFaker.Create" />. Fixtures that use the helper own
/// a private stream and do not depend on this value; a shared stream is order-dependent
/// under parallel execution and cannot be relied on to reproduce a specific failure.
/// </summary>
internal static class TestDataModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize() => Randomizer.Seed = new Random(Seed: 116116);
}
```

Close the uniqueness gap in `UserBuilder` while the file is open. Both columns it
derives from the randomizer carry unique indexes in production
(`src/Modules/Identity/Identity/Infrastructure/Persistence/Configurations/UserConfiguration.cs:54-55`),
and neither has a GUID component today, unlike the ten content builders that already
guarantee uniqueness structurally:

```csharp
// tests/Fixtures/Builders/Entities/Identity/UserBuilder.cs:31-43 — after
public UserBuilder()
{
    _id = Guid.NewGuid();
    _email = $"{_faker.Internet.UserName()}.{Guid.NewGuid():N}@example.test".ToLowerInvariant();

    string suffix = Guid.NewGuid().ToString("N")[..6];
    string generatedName = $"{_faker.Name.FirstName()}{suffix}";
    _userName = generatedName[..Math.Min(generatedName.Length, TestConstants.User.UserNameMaxLength)];

    _passwordHash = TestConstants.User.DefaultPasswordHash;
}
```

That truncation reads the aliased constant from
[03-constant-aliasing.md](03-constant-aliasing.md); if this change lands first, the
username is truncated to 50 and the real column limit is 20, so land 03 first or expect
the fix to be incomplete until it does.

**If this is done wrong** — if `Randomizer.Seed` is deleted rather than kept — any
`Faker` still constructed with `new()` becomes fully non-deterministic instead of merely
order-dependent, which is worse than the state being fixed.

## Expected fallout

**Tests that depended on another test's leftovers will fail.** That is the deliverable.
The likeliest shapes:

- A test that scripted `StubStreamingLinkResolutionService` and relied on a neighbouring
  test having set the same hook. After Change 1 each test starts from the default
  result and must script what it needs.
- A test that asserted on `DeletedPublicIds` or `Sent` containing an item another test
  put there. `Should().Contain(...)` over an accumulator that is now per-test may find
  nothing.
- A localization assertion that passed because a previous test had already set the
  culture it wanted. After Change 3 each test sets its own.

None of these are fixed by restoring the shared state. Fix the arrangement in the
failing test.

**`EmailDeliveryFlowTests` gets stronger, not just cleaner.** The baseline read at
`:307` and the `BeGreaterThan(alreadySent)` at `:317` exist only because `Sent` is never
cleared. With the reset in place:

```csharp
// tests/Integration/Workflows/EmailDeliveryFlowTests.cs — after
await job.Execute(new TestJobExecutionContext());

stub.Sent.Should().ContainSingle(m => m.To.Address == "drain@example.com" && m.Subject == "Drain me");
```

Do not keep the baseline "just in case". A baseline read is a test declaring that it
does not control its own arrangement.

**The six `ExternalAssetCleanupFlowTests` arm sites need no `finally`.** Lines 68, 101,
136, 165, 196 and 238 arm `NextDeleteFailure` outside any `try`. After Change 1 a leaked
one-shot cannot reach the next test, so the missing `finally` stops being a defect. Do
not add one.

**Peak connection count against the container drops.** Change 2 releases roughly 1,189
scopes per run that previously held their services for the whole session. Measure before
and after; the number is the evidence that the change did something.

**A small number of unit tests may change timing.** Change 5 gives each fixture a
different stream than it had, so any test that happened to depend on a specific
generated value — a name length, a particular email domain — will produce a different
one. Any test that breaks this way was asserting on random data and needs an explicit
value instead.

**Nothing here should change a production behaviour.** Every file in this spec is under
`tests/`. If a change to `src/` looks necessary, it belongs in another spec.

## Testing

```bash
dotnet build
dotnet test tests/Unit
dotnet test tests/Integration
```

Determinism is the actual claim, so run each suite twice and compare:

```bash
dotnet test tests/Unit && dotnet test tests/Unit
dotnet test tests/Integration && dotnet test tests/Integration
```

Identical results both times, including which tests ran and which passed.

Grep-provable invariants:

```bash
# No test resets a stub by hand.
grep -rn "StubStreamingLinkResolutionService.Reset\|StubEmailSender.*\.Reset()" tests/Integration

# No test assigns ambient culture directly.
grep -rn "Thread.CurrentThread.CurrentCulture\|Thread.CurrentThread.CurrentUICulture" tests/

# No fixture builds a Faker on the shared randomizer.
grep -rn "Faker _faker = new()" tests/

# Every class that mutates the environment is in the collection.
grep -rln "Environment.SetEnvironmentVariable" tests/Unit
```

The first three must return nothing. Every file in the fourth must carry
`[Collection("EnvironmentVariable")]`; check that with a follow-up grep per file rather
than by eye.

New evidence the changes worked:

- Arm `StubCloudinaryService.NextDeleteFailure` in a scratch test, let it fail before
  the failure is consumed, and confirm the next test in the same class is unaffected.
  Delete the scratch test afterwards.
- Add a temporary counter to `OpenScope` and assert in `DisposeAsync` that the disposed
  count equals the opened count for one heavily-scoped test class, then remove it.
- Run the localization theories in isolation and then interleaved with an
  English-asserting test class; both orders must give the same results.

## Risks

**The 104-file culture edit is the largest mechanical change in the audit.** Mitigation:
land Change 3 as its own commit, per test project area, and rely on the grep invariant
rather than review to prove completeness. Do not combine it with spec 06's rewrite of
the same files — if spec 06 deletes a file, this change was wasted on it, so agree the
order with whoever owns 06 first.

**Scope disposal could surface use-after-dispose in a test that stores a context in a
field.** Mitigation: those failures are immediate and loud (`ObjectDisposedException`),
and the fix is to resolve the context inside the test rather than hold it across tests.

**Moving the `SuperAdmin*` classes into `EnvironmentVariable` widens a
non-parallelised collection.** Five classes become nine, and all of them run serially.
Mitigation: measure the unit suite wall clock before and after; if it matters, the real
fix is spec 09's injected options, which empties the collection rather than growing it.

**Singleton registration for `StubStreamingLinkResolutionService` changes its lifetime.**
It currently resolves per scope. Nothing in the stub depends on scoped services — it
holds two properties and a dictionary — but confirm that against the file before
changing the registration.

**`TestFaker` gives a different stream to each fixture than the shared randomizer did.**
Mitigation: this is intentional and one-time. Re-run the unit suite twice after the
change; a value-dependent test fails identically both times, which distinguishes it from
a genuine flake.

## Checklist

- [ ] 1 — `IResettableStub` added; the three stubs implement it; all three registered
      under the interface as singletons; `ResetStubs()` called from both base classes;
      13 hand-written `Reset()` calls and 8 static hook assignments removed
- [ ] 2 — `_scopes` and `OpenScope()` added to both base classes; the four helpers route
      through it; both `DisposeAsync` implementations dispose every tracked scope,
      preferring `IAsyncDisposable`; the three already-correct sites left alone
- [ ] 3 — `CultureScope` restores both cultures; all 104 files use it; no direct
      assignment to `Thread.CurrentThread.CurrentCulture` remains
- [ ] 4 — Eight classes carry `[Collection("EnvironmentVariable")]`; the
      `SuperAdminSeeder` collection definition deleted; four classes gained a restoring
      `Dispose` that restores `null` correctly
- [ ] 5 — `TestFaker.Create()` added; all 73 declarations call it;
      `TestDataModuleInitializer` kept with a corrected doc comment; `UserBuilder`
      derives email and username with a GUID component
- [ ] `EmailDeliveryFlowTests` asserts exact counts instead of reading a baseline
- [ ] Both suites run twice back to back with identical results
