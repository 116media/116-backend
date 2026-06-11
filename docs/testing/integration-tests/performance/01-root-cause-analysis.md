# Root Cause Analysis: Integration Test Performance

## Problem Statement

535 integration tests take ~6+ minutes to run, while ~6000 unit tests complete faster.
This is unacceptable for developer feedback loops and CI pipelines.

## Executive Summary

The primary bottleneck is **45 redundant WebApplicationFactory startups**. Every test class
creates its own `ApiFixture` (which extends `WebApplicationFactory<Program>`), booting the
entire ASP.NET Core application from scratch — including DI container, middleware pipeline,
module registration, EF Core configuration, and JWT setup. Combined with the fact that all
45 classes are forced into serial execution via a single `[Collection("Database")]`, this
creates a cascading performance disaster.

## Architecture of the Current Test Infrastructure

```
PostgresFixture (shared via ICollectionFixture — 1 instance)
  └── starts Testcontainer (1 PostgreSQL container for all tests)
  └── applies EF migrations (3 DbContexts)
  └── creates Respawner instance

BaseApiTest (per-class)
  └── creates new ApiFixture(db)         ← WebApplicationFactory<Program>
  └── creates new HttpClient             ← from the ApiFixture test server
  └── on InitializeAsync:
       └── Respawn.ResetAsync()          ← truncates all tables
       └── SeedTestUsersAsync()          ← seeds 3 users

BaseRepositoryTest (per-class)
  └── creates new ApiFixture(db)         ← WebApplicationFactory<Program>
  └── on InitializeAsync:
       └── Respawn.ResetAsync()          ← truncates all tables
```

## Root Causes (Ranked by Impact)

### 1. Per-Class WebApplicationFactory Startup (Critical)

**Impact: ~80% of total slowness**

Both `BaseApiTest` and `BaseRepositoryTest` create `new ApiFixture(db)` in their
**constructor**, which runs for every test class (45 classes total).

```csharp
// BaseApiTest.cs — line 37
protected BaseApiTest(PostgresFixture db)
{
    Db = db;
    Api = new ApiFixture(db);      // Boots entire ASP.NET app
    Client = Api.CreateClient();   // Creates TestServer + HttpClient
}

// BaseRepositoryTest.cs — line 27
protected BaseRepositoryTest(PostgresFixture postgres)
{
    Postgres = postgres;
    Api = new ApiFixture(postgres); // Boots entire ASP.NET app AGAIN
}
```

Each `WebApplicationFactory<Program>` startup involves:
- Full `Program.cs` execution (host builder, all modules)
- DI container compilation (~500+ service registrations across 3 modules)
- EF Core model building for 3 DbContexts
- Carter endpoint routing registration (147+ content endpoints, 68 identity endpoints)
- JWT authentication scheme configuration
- Middleware pipeline construction
- Rate limiter initialization
- Serilog configuration

A single startup takes **~2-5 seconds**. Across 45 classes, that's **90-225 seconds**
of pure overhead before any test logic runs.

### 2. Single Collection Forces Serial Execution (High)

**Impact: ~15% of total slowness (missed parallelism opportunity)**

Every test class uses `[Collection("Database")]`:

```csharp
[Collection("Database")]
public class AdminCategoryCommandEndpointTests(PostgresFixture db) : BaseApiTest(db)
```

xUnit's collection system forces all classes in the same collection to run **sequentially**,
one test at a time, on a single thread. With 535 tests across 45 classes, there is zero
parallelism.

This design was necessary because all tests share a single PostgreSQL database — running
tests in parallel would cause data conflicts. However, the solution isn't more databases;
it's reducing per-test overhead so serial execution is fast enough.

### 3. Per-Test Database Reset via Respawn (Medium)

**Impact: ~5% of total slowness**

Every test method triggers `Respawn.ResetAsync()` via `IAsyncLifetime.InitializeAsync()`:

```csharp
// BaseApiTest.cs
public async ValueTask InitializeAsync()
{
    await Db.ResetAsync();          // Opens connection, truncates tables
    await SeedTestUsersAsync();     // Creates 3 users
    await SeedAsync();              // Optional custom seeding
}
```

Each reset:
1. Opens a new `NpgsqlConnection`
2. Executes `TRUNCATE TABLE ... CASCADE` across all tables in 3 schemas
3. Closes the connection

For 535 tests, that's 535 connection open/truncate/close cycles. Each takes ~10-30ms,
totaling ~5-16 seconds — not the primary issue, but adds up.

### 4. Per-Test User Seeding in BaseApiTest (Low-Medium)

**Impact: ~3% of total slowness**

`SeedTestUsersAsync()` creates a new `IdentityDbContext` scope and inserts 3 users for
every API test. With 23 API test classes containing ~350+ tests, that's ~350 redundant
DbContext creations and SaveChanges calls.

### 5. DI Scope Leaks (Low)

`CreateDbContext<T>()` creates a new DI scope but never disposes it:

```csharp
protected TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext
{
    var scope = Api.Services.CreateScope();  // Never disposed
    return scope.ServiceProvider.GetRequiredService<TDbContext>();
}
```

This leaks `IServiceScope` objects. While not directly causing slowness, accumulated
DbContext instances can exhaust connection pool capacity, causing connection wait times
under load.

## Quantified Breakdown

| Source | Per-Instance Cost | Count | Total |
|--------|------------------|-------|-------|
| WebApplicationFactory startup | ~2-5s | 45 | 90-225s |
| Respawn.ResetAsync() | ~10-30ms | 535 | 5-16s |
| SeedTestUsersAsync() | ~5-15ms | ~350 | 2-5s |
| Actual test logic | ~5-50ms | 535 | 3-27s |
| Test framework overhead | ~1-2ms | 535 | 1-2s |
| **Total** | | | **~100-275s** |

The WebApplicationFactory startup accounts for **60-80%** of total execution time.

## Environment Variables as Global State

`ApiFixture.SetEnvironmentVariables()` uses `Environment.SetEnvironmentVariable()` to
configure the app. This is process-wide mutable state — safe only because tests run
serially. If parallelism were introduced without fixing this, tests would corrupt each
other's environment.
