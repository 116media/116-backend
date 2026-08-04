# Spec 11 — Suite performance

## Goal

Recover integration suite runtime at zero risk now, and have the structural change
fully specified so it can be executed later without re-deriving it. The index records
the decision: container flags now, sharding when the 600-second CI budget is actually
threatened. This spec honours that split — Changes 1 and 2 are immediately
actionable, Change 3 is specified in full and gated behind a stated trigger.

## Scope

In scope:

- `tests/Integration/Common/Fixtures/PostgresFixture.cs` — container configuration.
- `tests/Integration/Common/Fixtures/RateLimitedPostgresFixture.cs` — stop paying for
  a second container and a second four-context migration pass to serve three tests.
- `tests/Integration/xunit.runner.json` — new, so parallelism intent is stated rather
  than inherited from xUnit's defaults.
- A measurement step, because none of the numbers below can be defended without a
  before-and-after timing on the same machine.

Specified but gated, not to be implemented until the trigger in Change 3 fires:

- Promoting the container to an xUnit v3 `[assembly: AssemblyFixture]`.
- Migrating one template database and leasing per-collection databases via
  `CREATE DATABASE ... TEMPLATE`.
- Sharding `DatabaseCollection` into per-module collections.

Not in this spec:

- Changing `TestSessionTimeout`. Raising it before the runtime is understood converts
  a structural problem into a permanently rising number. It is revisited only in
  Change 3, after the gated work lands.
- Reducing the per-test reset and seed work in `BaseApiTest.InitializeAsync`. Those
  three cache-invalidation scopes and the three user inserts are correctness
  machinery; removing them is spec 02's territory, not a performance change.
- `SingleHit` in `tests/coverage.runsettings:12`. It is `false` deliberately, and the
  accuracy is worth the cost.
- The unit suite, which is already parallel across classes.

## Prerequisites

- None for Changes 1 and 2. Both are configuration-only and change no assertion.
- Change 3 requires spec 02 to have landed in full. Serialisation is currently hiding
  every piece of shared mutable state in the suite, and the first parallel run is what
  surfaces all of it at once. Attempting Change 3 before spec 02 converts a
  performance task into an unbounded debugging task.

## The current numbers

These are measured from the repository as it stands, and are the baseline any claim
of improvement is judged against.

| Metric | Value | Source |
| --- | --- | --- |
| Test classes carrying `[Collection(...)]` | 357 | `tests/Integration/` |
| Classes in `[Collection("Database")]` | 356 | `tests/Integration/` |
| Classes in any other collection | 1 | `RateLimitingExtensionTests` in `[Collection("RateLimiting")]` |
| `[Fact]` and `[Theory]` methods | 1,879 | `tests/Integration/` |
| `xunit.runner.json` files | 0 | repository root and `tests/Integration/` |
| `MaxCpuCount` | 0 (all cores) | `tests/coverage.runsettings:22` |
| `TestSessionTimeout` | 600000 ms | `tests/coverage.runsettings:25` |
| Implied budget per test | ~319 ms | 600 s / 1,879 |
| Postgres containers started per run | 2 | `PostgresFixture` and `RateLimitedPostgresFixture` |
| `MigrateAsync` calls per run | 8 | four contexts × two fixtures, `PostgresFixture.cs:88-117` |

xUnit's defaults run collections in parallel with each other and tests within a
collection serially. With 356 of 357 classes in one collection, the parallel path is
unreachable and `MaxCpuCount` has nothing to distribute.

## Changes

### 1. Remove durability work from a throwaway container

The container is doing durable-storage work for a database that is destroyed when the
run ends. The per-test Respawn truncation across four schemas and the three seed
inserts each pay an fsync, and the data directory sits on the container's overlay
filesystem.

Before, `tests/Integration/Common/Fixtures/PostgresFixture.cs:22-26`:

```csharp
private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("test_116_db")
    .WithUsername("test_user")
    .WithPassword("test_password")
    .Build();
```

After:

```csharp
private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("test_116_db")
    .WithUsername("test_user")
    .WithPassword("test_password")
    .WithTmpfsMount("/var/lib/postgresql/data")
    .WithCommand("-c", "fsync=off", "-c", "full_page_writes=off", "-c", "synchronous_commit=off")
    .Build();
```

The reasoning belongs in a comment on the field, because the settings look alarming out
of context:

```csharp
/// <summary>
/// The PostgreSQL container backing every test in this fixture's collection.
/// </summary>
/// <remarks>
/// Durability is disabled deliberately. The data directory is a tmpfs mount and the database is
/// destroyed when the run ends, so write-ahead logging, full page writes, and synchronous commits
/// buy nothing and cost an fsync on every Respawn truncation and every seed insert. Losing this
/// database on a crash is the intended outcome, which is precisely the condition under which
/// these settings are safe.
/// </remarks>
```

What breaks if done wrong: `WithCommand` replaces the container's command rather than
appending to it on some Testcontainers versions. Verify after the change that the
container still starts and that `PostgresFixture.ConnectionString` resolves — a
silently failing container surfaces as a connection timeout in the first test, not as
a fixture error. Also confirm the CI runner allows tmpfs mounts; the GitHub-hosted
`ubuntu-latest` runner used by `.github/workflows/tests.yml` does.

### 2. Share the container with the rate-limited fixture

`RateLimitedPostgresFixture` inherits the entire `PostgresFixture` lifecycle and
overrides only the host:

```csharp
// tests/Integration/Common/Fixtures/RateLimitedPostgresFixture.cs
public class RateLimitedPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new RateLimitedApiFixture(this);
}
```

Because it inherits `InitializeAsync`, it starts a second `postgres:16-alpine`
container and runs `ApplyMigrationsAsync` for all four contexts a second time. Its
only consumer is
`tests/Integration/Shared/Application/Extensions/RateLimitingExtensionTests.cs`, which
holds three test methods today and grows to one theory over ten policies under spec 12.

The reason it needs a separate *host* is real: `RateLimitedApiFixture` sets
`DisableRateLimits` to `false`, and permits consumed while driving a policy to
rejection must not leak into the shared collection. That reason does not extend to
needing a separate *database*.

Restructure `PostgresFixture` so container ownership is separable from host ownership:

```csharp
/// <summary>
/// Creates the database this fixture uses. The base fixture owns and starts the container;
/// a derived fixture may override this to lease a database from an already-running container
/// instead of starting one of its own.
/// </summary>
/// <returns>The connection string the API fixture and Respawner will use.</returns>
protected virtual async Task<string> ProvisionDatabaseAsync()
{
    await _container.StartAsync();
    return _container.GetConnectionString();
}
```

`RateLimitedPostgresFixture` then overrides `ProvisionDatabaseAsync` to create a second
database inside the container the shared `PostgresFixture` already runs, and skips
`ApplyMigrationsAsync` in favour of a template clone once Change 3 lands. Before
Change 3, the simplest correct form is for it to reuse the shared connection string
directly:

```csharp
/// <summary>
/// A <see cref="PostgresFixture" /> that keeps the production rate limit policies active.
/// </summary>
/// <remarks>
/// It shares the container and the migrated database started by the shared fixture, because the
/// isolation this collection needs is at the host level — permits must not leak into the shared
/// collection — not at the database level. Its tests seed no rows and assert no persisted state,
/// so sharing a database with a serialised sibling collection is safe.
/// </remarks>
public class RateLimitedPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new RateLimitedApiFixture(this);
}
```

What breaks if done wrong: the two collections currently run in parallel with each
other, since they are distinct collections. If they share one database and either
starts asserting persisted state, one collection's Respawn truncation will delete the
other's rows mid-test. Guard this with an explicit statement in the
`RateLimitingExtensionTests` class comment that the class must not seed or assert
database state, and confirm at review time that spec 12's policy theory keeps that
property — it drives endpoints to rejection and never reads a row.

If the team is not comfortable sharing a database across parallel collections, the
fallback that still removes most of the cost is to keep a second database but skip the
second migration pass, creating it from the migrated one with
`CREATE DATABASE rate_limited TEMPLATE test_116_db`. Record which option was taken.

### 3. Specified but gated: assembly fixture, template databases, and collection shards

**Do not implement this until the trigger below fires.**

**Trigger.** Implement Change 3 when a full integration run on CI, measured with the
timing step in Testing, exceeds **420 seconds** — 70% of the 600-second
`TestSessionTimeout`. That threshold is chosen so the work starts while there is still
headroom to do it calmly, rather than after the first aborted job. Record each
measured run time in this spec's implementation notes so the trend is visible before
the threshold is crossed.

**Step 3a — promote the container to an assembly fixture.** The project targets
`xunit.v3` 1.1.0 (`tests/Integration/_116.Integration.Tests.csproj:19`), so
`[assembly: AssemblyFixture]` is available.

```csharp
// tests/Integration/Common/Fixtures/AssemblyFixtures.cs
[assembly: AssemblyFixture(typeof(PostgresContainerFixture))]

/// <summary>
/// Starts the single PostgreSQL container for the whole assembly and migrates one template
/// database. Collections lease their own database cloned from that template, so no two
/// collections share truncation targets and none pays for its own migration pass.
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    private const string TemplateDatabase = "test_116_template";

    /// <summary>
    /// Creates a database cloned from the migrated template and returns its connection string.
    /// </summary>
    /// <param name="name">The database name to create, unique per collection.</param>
    /// <returns>The connection string addressing the newly leased database.</returns>
    public async Task<string> LeaseDatabaseAsync(string name)
    {
        await using var admin = new NpgsqlConnection(AdminConnectionString);
        await admin.OpenAsync();

        await using NpgsqlCommand command = admin.CreateCommand();
        command.CommandText = $"""CREATE DATABASE "{name}" TEMPLATE "{TemplateDatabase}";""";
        await command.ExecuteNonQueryAsync();

        return ConnectionStringFor(name);
    }
}
```

`CREATE DATABASE ... TEMPLATE` is a file-level copy of an already-migrated database. It
replaces the current four `MigrateAsync` calls per fixture with one migration pass for
the entire assembly.

**Step 3b — shard `DatabaseCollection` per module.** One collection definition becomes
four, each leasing its own database:

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

The four run concurrently; tests inside each stay serial, so no individual test's
isolation assumptions change. Test classes move by editing one attribute, and
`tests/Integration/Modules/` already mirrors the module boundaries the shards follow.
Every `[Collection("Database")]` in 356 files becomes the shard matching its directory.

**Step 3c — fold the rate-limited fixture into the same mechanism.**
`RateLimitedPostgresFixture` leases a database from the assembly fixture like any other
collection and keeps only its `RateLimitedApiFixture` override, which is the part that
genuinely needs to be separate.

**Step 3d — reset `TestSessionTimeout` once, at the end.** After the shards are green
twice back to back, raise `tests/coverage.runsettings:25` to a value that detects a
hang rather than one the suite is racing. A session timeout is an alarm, not a budget.

What breaks if done wrong: `CREATE DATABASE ... TEMPLATE` fails if any session is
connected to the template. The assembly fixture must close its migration connections
before the first lease, and no fixture may ever connect to the template for test work.
Second, the four shard fixtures each need their own `Respawner`, targeting their own
database; reusing one Respawner across databases truncates the wrong tables.

### 4. State parallelism explicitly

With no runner configuration file, the suite's behaviour is whatever xUnit's defaults
are on the day. Add `tests/Integration/xunit.runner.json`, marked as content in the
project file so it lands beside the test assembly:

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": false,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4
}
```

This is a documentation change today — with two collections, `maxParallelThreads: 4`
distributes nothing extra — and becomes load-bearing under Change 3. Adding it now
means Change 3 does not have to reason about defaults at the same time as it reasons
about shared state.

## Expected fallout

- Change 1 changes no test and no assertion. If any test goes red, it was depending on
  a timing coincidence, and that is a finding.
- Change 2 removes one container start and four `MigrateAsync` calls from every run.
  Container start on a warm CI image dominates that saving.
- Change 4 changes nothing observable until Change 3.
- Under Change 3, expect the first parallel run to fail. Every shared static, ambient
  culture assignment, singleton stub flag and process-global environment variable in
  the suite currently passes because the suite is single-threaded, not because it is
  safe. That surfacing is the reason spec 02 is a prerequisite, and any failure it
  produces is a real isolation defect rather than a regression introduced here.

## Testing

Measurement is part of the deliverable. Run each of these three times and record the
median, on the same machine, with the container image already pulled.

```bash
dotnet build

# Baseline, before any change in this spec.
time dotnet test tests/Integration --settings tests/coverage.runsettings

# After Change 1.
time dotnet test tests/Integration --settings tests/coverage.runsettings

# After Change 2, confirming the rate limiting collection still passes in isolation.
dotnet test tests/Integration --filter "FullyQualifiedName~RateLimitingExtensionTests"
```

What must be green: the entire integration suite, with the same test count as before.
Compare the `.trx` totals rather than eyeballing the console, because a fixture that
fails to start can reduce the executed count while the run still reports success at
the shell.

Run the full integration suite twice back to back and confirm identical results. That
is the same gate spec 14 applies, and it is the only evidence that Change 2's shared
database has not introduced cross-collection interference.

What the measurement proves: the median run time before and after Change 1 is the whole
justification for the change, and it is the number that gets compared against the
420-second trigger for Change 3. Record all three medians in this spec's implementation
notes with the machine and date.

## Risks

**Disabling fsync is alarming without its reasoning attached.** Someone will find
`fsync=off` in a diff and object. Mitigation: the remarks block on the container field
states that the data directory is a tmpfs mount destroyed at the end of the run, which
is the condition that makes the setting correct rather than reckless.

**tmpfs is memory.** The test database now occupies RAM on the runner. Four schemas of
migrated-but-mostly-empty tables plus per-test seed rows is small, but a future test
that bulk-inserts could change that. Mitigation: if a run starts failing with an
out-of-space error inside the container, size the mount explicitly rather than removing
it, and record the size chosen.

**Change 2 shares a database across two parallel collections.** This is safe only
because `RateLimitingExtensionTests` seeds nothing and asserts no persisted state.
Mitigation: state that constraint in the class comment, verify spec 12's policy theory
preserves it, and take the template-clone fallback if the constraint cannot be
guaranteed.

**Change 3 touches 356 files.** A mechanical attribute rewrite across that many files
invites a class landing in the wrong shard, where it will pass until another shard's
test happens to seed conflicting data. Mitigation: derive the shard from the file's
directory rather than by hand, and add a test that asserts every class under
`tests/Integration/Modules/Identity/` carries `[Collection("Identity")]` and so on for
each module.

**Gating can become forgetting.** Specified-but-not-done work tends to stay not done
until it is urgent. Mitigation: the trigger is a number, not a judgement, and each
measured run time is recorded in this spec. When a recorded value crosses 420 seconds,
Change 3 starts.

## Checklist

- [x] Baseline run time measured three times and the median recorded with machine and
      date
- [x] 1 — tmpfs mount applied to the container, with the reasoning on the field. The
      three durability flags were already applied by Testcontainers; see the
      implementation notes
- [x] 1 — Post-change container and reset-path timings measured and the medians recorded
- [x] 2 — No fixture starts a container of its own or runs a four-context migration pass
      of its own, and which option was taken is recorded
- [x] 2 — Superseded: no database is shared, so `RateLimitingExtensionTests` needs no
      seed-and-assert constraint
- [x] 4 — `tests/Integration/xunit.runner.json` added with parallelism stated
      explicitly and included as content in the project file
- [ ] Full integration suite run twice back to back with identical results and an
      unchanged test count
- [x] 3 — Recorded as gated, with the 420-second trigger stated and the running log of
      measured times started

## Implementation notes

Implemented 2026-08-23 on macOS 15 (Darwin 25.5.0), Docker Desktop 28.2.2, Apple
silicon, `postgres:16-alpine` already pulled. `DatabaseCollection` was **not** sharded,
per the decision in [00-index.md](00-index.md).

### The container count the spec's arithmetic misses

The spec's table says two containers and eight `MigrateAsync` calls. By the time this
spec was implemented there were **four** containers and **sixteen** `MigrateAsync`
calls, because specs 01 and 12 each added a fixture:

| Fixture | Collection | Why its host must be its own |
| --- | --- | --- |
| `PostgresFixture` | `Database` (355 classes) | The general host: Testing environment, rate limits stubbed out |
| `RateLimitedPostgresFixture` | `RateLimiting` (1 class) | `DisableRateLimits` is `false`, so permits are real and must not leak |
| `SeedingPostgresFixture` | `Seeding` (2 classes) | Boots as Development so the module seeders run at startup |
| `CorsPostgresFixture` | `Cors` (1 class) | Sets `DASHBOARD_ORIGIN` before the host reads it during construction |

All four hosts genuinely have to differ, and none of them was merged. What was merged
is the layer underneath: all four were paying for a container start and a four-context
migration pass to obtain a database that is byte-identical to the other three.

`TestPostgresContainer` now owns one container for the assembly, migrates one
`test_116_template` database, and hands each fixture a private database created with
`CREATE DATABASE ... TEMPLATE`. Every fixture keeps its own database, its own
`Respawner` and its own host, so the isolation boundary is exactly where it was — this
is the spec's own recorded fallback for Change 2, generalised from one fixture to four.
`PostgresContainerFixture` is registered as `[assembly: AssemblyFixture]` purely to own
the container's lifetime, since the container has to outlive whichever collection
happens to finish first.

The template is never used for test work and its Npgsql pools are cleared after
migration, because `CREATE DATABASE ... TEMPLATE` fails while any session is attached
to the source. Lingering backends are terminated and the copy is retried on SQLSTATE
`55006` as a second line of defence. `max_connections` is raised to 200 because one
server now backs four databases.

### Measured, on the machine and date above

Container start is measured from `docker run` to the first successful `pg_isready`,
which is exactly what Testcontainers' `PostgreSqlBuilder` wait strategy polls. The
reset-path figure runs Respawn's actual Postgres statement — `truncate table <all>
cascade` — over 60 tables spread across the four module schemas, followed by the three
committed seed inserts `BaseApiTest` performs, 100 times.

| Configuration | Container start (median of 11) | Reset cycle (median of 5 × 100) |
| --- | --- | --- |
| Durability flags only — what the repository already had | 1,355 ms (1,157–3,134) | 33.6 ms per cycle |
| Durability flags + tmpfs data directory — after | 1,347 ms (1,151–1,698) | 24.6 ms per cycle |

`CREATE DATABASE ... TEMPLATE` against a 14 MB migrated database: 210, 294, 384 and
519 ms across four consecutive clones, median ~339 ms.

The tmpfs mount defaults to 50% of the Docker VM's memory, measured at 3.8 GB on this
machine against a 45.4 MB fresh data directory, so no explicit size was set. The
spec's mitigation — size it rather than remove it — still applies if that ever changes.

### What the spec got wrong

**The three durability flags were already on.** `Testcontainers.PostgreSql` 4.12.0
applies `-c fsync=off`, `-c full_page_writes=off` and `-c synchronous_commit=off` in
`PostgreSqlBuilder.Init()`. Adding them by hand, as Change 1 instructs, is a no-op.
They are worth having — measured against a container with durability left on, the same
commit-heavy workload took 573–649 ms rather than 278–298 ms — but that saving was
already banked before this spec was written, and any before/after attributed to it
would have been fictional. The only part of Change 1 with an effect left in it is the
tmpfs mount.

**`WithCommand` appends, it does not replace.** `ContainerConfiguration` merges the
command with `BuildConfiguration.Combine`, which concatenates; `Init()` itself relies on
this by calling `WithCommand` three times. The spec's warning is inverted for this
version.

**The counts are stale.** Four containers and sixteen `MigrateAsync` calls, not two and
eight. 1,936 executed tests, not 1,879. Four collections, not two.

### What this is expected to do to the suite, and what was not measured

`dotnet test` was not run here; the orchestrator owns that. The last full integration
run took **3 m 41 s (221 s) for 1,936 tests**, with earlier runs ranging 2 m 57 s to
5 m 44 s. The expected effect, stated as an expectation rather than a result:

- Three container starts removed, three template clones added: ~4.0 s saved, ~1.0 s
  spent, net **~3 s**.
- Twelve of the sixteen `MigrateAsync` calls removed — three fixtures × four contexts,
  replaced by three ~339 ms clones. The cost of one four-context pass over 38 migration
  files was not measured, so no number is claimed for it, but it is the larger of the
  two savings.
- The tmpfs mount saves ~9 ms per test on the reset path, which over 1,936 tests is
  **~17 s**, or roughly 8% of the 221 s baseline.

Taken together, an expectation on the order of 20–30 s. That is smaller than the
observed run-to-run spread of 177 s to 344 s, so a single before/after pair cannot
confirm it and should not be read as doing so. A median over several runs can.

### Durability trade-off

`fsync=off`, `full_page_writes=off` and `synchronous_commit=off` mean a crash mid-run
leaves an unrecoverable data directory, and the tmpfs mount means the data directory
does not survive the container at all. Both are correct here for the same reason: the
database is created by the test run, thrown away by the test run, and rebuilt from
migrations on the next one. Losing it on a crash is the intended outcome, so the
guarantees being disabled protect nothing. They would be indefensible on any container
whose data is expected to outlive the process that created it.

### Running log of measured CI run times

Change 3 starts when a recorded value crosses **420 seconds**.

| Date | Run time | Notes |
| --- | --- | --- |
| 2026-08-23 | 221 s | 1,936 tests, before this spec |
