# High — The suite is fully serialised against a timeout cliff

Every database-backed integration test in the project belongs to one xUnit
collection, and xUnit never runs tests within a collection in parallel. All 1,879
tests therefore execute strictly one after another, on a CI run configured with a
600-second hard abort. That is roughly 319 milliseconds per test, inclusive of a
four-schema Respawn truncation, three DI scopes, three user inserts, HTTP round
trips and coverlet instrumentation. The suite is not slow because the tests are
slow; it is slow because it has been told it may only ever use one core.

## The problem

One collection definition, one fixture, no sharding:

```csharp
// tests/Integration/Common/Fixtures/DatabaseCollection.cs:7-8
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgresFixture>;
```

Measured across `tests/Integration/`:

| Metric | Value |
| --- | --- |
| Test classes carrying `[Collection(...)]` | 357 |
| Classes in `[Collection("Database")]` | 356 |
| Classes in any other collection | 1 (`RateLimitingExtensionTests`) |
| `[Fact]` / `[Theory]` methods | 1,879 |
| `xunit.runner.json` files | 0 |
| `[assembly: CollectionBehavior(...)]` declarations | 0 |

With no runner configuration and no assembly attribute, xUnit's defaults apply:
collections run in parallel with each other, tests inside a collection do not. With
356 of 357 classes in one collection, the parallel path is unreachable. The
`MaxCpuCount` of `0` at `tests/coverage.runsettings:22` — meaning "use all
available cores" — has nothing to distribute.

CI runs with those settings applied:

```yaml
# .github/workflows/tests.yml:87-93
dotnet test tests/Integration \
  --configuration Release \
  --no-restore \
  --settings tests/coverage.runsettings \
  --collect:"XPlat Code Coverage" \
  ...
```

```xml
<!-- tests/coverage.runsettings:25 -->
<TestSessionTimeout>600000</TestSessionTimeout>
```

`TestSessionTimeout` is not a per-test budget. It aborts the whole session, and an
aborted session reports as a failed job with no useful attribution — the run does
not tell you which test was slow, only that the wall clock ran out.

**What each test pays for before it does anything.** Per test method, via
`BaseApiTest.InitializeAsync` (`tests/Integration/Common/Base/BaseApiTest.cs:117-126`):

1. `Db.ResetAsync()` — a Respawn truncation across the `identity`, `core`,
   `content` and `mailer` schemas (`PostgresFixture.cs:126-133`), on a fresh
   `NpgsqlConnection` opened and closed each time (`PostgresFixture.cs:64-72`)
2. three `Api.Services.CreateScope()` calls to invalidate three memory caches
   (`BaseApiTest.cs:139, 152, 165`)
3. a fourth scope plus three `INSERT`s to seed the well-known users
   (`BaseApiTest.cs:181-199`)

Then the test body runs, under coverlet with `<SingleHit>false</SingleHit>`
(`coverage.runsettings:12`), which records every hit rather than stopping at the
first — more accurate, and measurably more expensive per line executed.

**A second container exists to serve three tests.** `RateLimitedPostgresFixture`
inherits the entire `PostgresFixture` lifecycle:

```csharp
// tests/Integration/Common/Fixtures/RateLimitedPostgresFixture.cs:8-12
public class RateLimitedPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new RateLimitedApiFixture(this);
}
```

It overrides only the host, so it still starts its own `postgres:16-alpine`
container and still runs `ApplyMigrationsAsync` for all four contexts
(`PostgresFixture.cs:88-117`). Its sole consumer is
`tests/Integration/Shared/Application/Extensions/RateLimitingExtensionTests.cs`,
which contains three test methods. The suite pays a second container start and a
second full migration pass for three tests — and the reason it needs a separate
*host* (permits must not leak into the shared collection) does not require a
separate *database*.

## Why it matters

At 1,879 tests against a 600-second abort, the budget is 319 ms per test with zero
headroom. Nothing in the current shape of the suite reduces that number over time,
and everything about its growth increases it: each new endpoint test adds a full
reset-and-seed cycle to a serial queue.

The failure mode when the cliff is reached is the bad kind. The job aborts, no
individual test is blamed, and the natural response under delivery pressure is to
raise `TestSessionTimeout` — which converts a structural problem into a permanently
rising number and hides every future regression in test time behind it.

Serialisation also removes a real safety property. Tests that only ever run one at a
time are never checked for the assumptions they make about being alone. Shared
static state, ambient culture, singleton stub flags
([05](05-shared-mutable-state.md)) and process-global environment variables
([04](04-production-wiring-divergence.md)) all pass today because the suite is
single-threaded, not because they are safe. That debt is invisible until the first
attempt to parallelise, at which point it all surfaces at once.

## The fix

Two tracks: a cheap change that costs nothing and can land today, and the
structural change that removes the ceiling.

### Immediate, no-risk: make the container fast

The container is doing durable-storage work for a database that is destroyed at the
end of the run. Disable it.

```csharp
// tests/Integration/Common/Fixtures/PostgresFixture.cs — before
private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("test_116_db")
    .WithUsername("test_user")
    .WithPassword("test_password")
    .Build();

// after — the data directory is disposable, so durability is pure overhead
private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("test_116_db")
    .WithUsername("test_user")
    .WithPassword("test_password")
    .WithTmpfsMount("/var/lib/postgresql/data")
    .WithCommand(
        "-c",
        "fsync=off",
        "-c",
        "full_page_writes=off",
        "-c",
        "synchronous_commit=off"
    )
    .Build();
```

This changes no test and no assertion. It removes the fsync on every commit and
moves the data directory off the container's overlay filesystem, which is where the
per-test Respawn truncation and the seed inserts spend most of their time. It is
safe precisely because losing this database on crash is the intended outcome.

### Structural: give collections their own databases

Serialisation exists because one database is shared. Give each collection its own,
and the constraint disappears.

**Step 1 — promote the container to an assembly fixture.** The project already
targets `xunit.v3` 1.1.0 (`tests/Integration/Integration.csproj:19`), so
`[assembly: AssemblyFixture]` is available:

```csharp
// tests/Integration/Common/Fixtures/AssemblyFixtures.cs
[assembly: AssemblyFixture(typeof(PostgresContainerFixture))]

/// <summary>
/// Starts the single PostgreSQL container for the whole assembly and migrates one
/// template database. Collections lease their own database cloned from that template,
/// so no two collections share truncation targets.
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = /* as above */;

    /// <summary>
    /// Creates a database cloned from the migrated template and returns its connection string.
    /// </summary>
    public async Task<string> LeaseDatabaseAsync(string name)
    {
        await using var admin = new NpgsqlConnection(AdminConnectionString);
        await admin.OpenAsync();

        await using var command = admin.CreateCommand();
        command.CommandText = $"""CREATE DATABASE "{name}" TEMPLATE "{TemplateDatabase}";""";
        await command.ExecuteNonQueryAsync();

        return ConnectionStringFor(name);
    }
}
```

`CREATE DATABASE ... TEMPLATE` is a file-level copy of an already-migrated database.
It replaces the current four `MigrateAsync` calls per fixture
(`PostgresFixture.cs:88-117`) with one migration pass for the entire assembly.

**Step 2 — shard `DatabaseCollection` per module.** One collection definition
becomes four, each leasing its own database:

```csharp
// tests/Integration/Common/Fixtures/ModuleCollections.cs
[CollectionDefinition("Identity")]
public class IdentityCollection : ICollectionFixture<IdentityPostgresFixture>;

[CollectionDefinition("Content")]
public class ContentCollection : ICollectionFixture<ContentPostgresFixture>;

[CollectionDefinition("Core")]
public class CoreCollection : ICollectionFixture<CorePostgresFixture>;

[CollectionDefinition("Workflows")]
public class WorkflowsCollection : ICollectionFixture<WorkflowsPostgresFixture>;
```

The four run concurrently; tests inside each stay serial, so no test's isolation
assumptions change. Test classes move by editing one attribute, and the directory
layout already mirrors the module boundaries the shards follow.

**Step 3 — share the container with the rate-limited fixture.**
`RateLimitedPostgresFixture` leases a database from the same assembly fixture
instead of starting a container and migrating four contexts of its own. It keeps its
own `RateLimitedApiFixture`, which is the part that actually needs to be separate.

Sequence matters: do these after [05](05-shared-mutable-state.md), because the first
parallel run is what will surface every piece of shared state the current
serialisation is hiding.

### Also worth doing

Set the runner configuration explicitly rather than relying on defaults, so the
intent is readable and a future change to xUnit's defaults cannot alter it:

```json
// tests/Integration/xunit.runner.json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": false,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4
}
```

And once runtime is under control, raise the abort threshold to a level that catches
a genuine hang rather than sitting just above the expected duration. A session
timeout should be an alarm, not a budget.

## The principle

**A test collection is a shared-resource boundary, not a filing system.** Putting
every test in one collection is the correct move when they share one mutable
database — and the wrong resource to share. Databases are cheap to create from a
template; cores are not cheap to leave idle.

The second principle: **a suite that has never run in parallel does not know whether
it can.** Serialisation is not a property tests earn, it is a constraint they are
placed under, and it silently permits every shared-state defect underneath it. The
value of sharding is not only speed — it is that shared state stops being invisible.

## Checklist

- [ ] tmpfs mount and `fsync=off` / `full_page_writes=off` /
      `synchronous_commit=off` on the container, with the run timed before and after
- [ ] Container promoted to `[assembly: AssemblyFixture]`, migrations run once into
      a template database
- [ ] Collections lease databases via `CREATE DATABASE ... TEMPLATE`
- [ ] `DatabaseCollection` sharded per module; every test class updated to its shard
- [ ] `RateLimitedPostgresFixture` shares the assembly container and keeps only its
      own `RateLimitedApiFixture`
- [ ] `xunit.runner.json` added with parallelism stated explicitly
- [ ] Full suite run twice with identical results before the timeout is changed
- [ ] `TestSessionTimeout` reset to a value that detects a hang, not one the suite
      is racing
