# Testcontainers PostgreSQL Fixture

## Purpose

The PostgreSQL fixture manages a disposable Docker container that runs a real PostgreSQL instance. It is shared across all tests in a collection to avoid the cost of starting a container per test class.

## New Constants

Integration tests introduce two new `TestConstants` partials alongside the existing ones in `tests/Fixtures/Constants/Shared/`. All configuration values are centralized — no hardcoded strings in fixtures.

### `TestConstants.Database.cs`

```csharp
namespace _116.Tests.Fixtures.Constants;

/// <summary>
/// Constants for integration test database configuration.
/// Used by PostgresFixture and ApiFixture to configure Testcontainers and environment variables.
/// </summary>
public static partial class TestConstants
{
    public static class Database
    {
        public const string Image = "postgres:16-alpine";
        public const string Name = "integration_tests";
        public const string User = "test_user";
        public const string Password = "test_password";
    }
}
```

Schema names are **not** duplicated here — they already exist as module constants:

- `IdentityConstants.SchemaName` → `"identity"`
- `CoreConstants.SchemaName` → `"core"`
- `ContentConstants.SchemaName` → `"content"`

### `TestConstants.ExternalServices.cs`

```csharp
namespace _116.Tests.Fixtures.Constants;

/// <summary>
/// Constants for stubbed external service configuration.
/// Used by ApiFixture to set environment variables for Cloudinary, CORS origins, and default user password.
/// </summary>
public static partial class TestConstants
{
    public static class ExternalServices
    {
        public const string CloudinaryCloudName = "test_cloud";
        public const string CloudinaryApiKey = "test_key";
        public const string CloudinaryApiSecret = "test_secret";
        public const string DashboardOrigin = "http://localhost:5173";
        public const string WebappOrigin = "http://localhost:3000";
        public const string DefaultUserPassword = "TestPassword123!";
    }
}
```

Both files go in `tests/Fixtures/Constants/Shared/`, alongside the existing `TestConstants.ApiRoutes.cs` and `TestConstants.ValidationMessages.cs`.

The JWT constants (`Jwt.ValidSecret`, `Jwt.ValidIssuer`, etc.) already exist in `TestConstants.Jwt.cs`.

## PostgresFixture

```csharp
using _116.Content.Domain.Constants;
using _116.Core.Domain.Constants;
using _116.Identity.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using static _116.Tests.Fixtures.Constants.TestConstants;

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Manages a PostgreSQL Testcontainer shared across a test collection.
/// Provides database creation, migration, and per-test reset via Respawn.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage(Database.Image)
        .WithDatabase(Database.Name)
        .WithUsername(Database.User)
        .WithPassword(Database.Password)
        .Build();

    private Respawner? _respawner;

    /// <summary>
    /// The connection string for the running PostgreSQL container.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Starts the container and applies all EF Core migrations.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await MigrateAsync<IdentityDbContext>();
        await MigrateAsync<CoreDbContext>();
        await MigrateAsync<ContentDbContext>();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude =
            [
                IdentityConstants.SchemaName,
                CoreConstants.SchemaName,
                ContentConstants.SchemaName,
            ],
        });
    }

    /// <summary>
    /// Resets all data in the database. Call this in test class constructors
    /// or setup methods to guarantee a clean state.
    /// </summary>
    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner!.ResetAsync(connection);
    }

    /// <summary>
    /// Stops and removes the container.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a DbContext and applies pending migrations.
    /// </summary>
    private async Task MigrateAsync<TDbContext>() where TDbContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = (TDbContext)Activator.CreateInstance(
            typeof(TDbContext), options)!;
        await context.Database.MigrateAsync();
    }
}
```

## Collection Definition

xUnit collections share a single fixture instance across multiple test classes. All integration tests that need the database should be in the same collection.

```csharp
namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the shared database collection. All integration test classes
/// that use [Collection("Database")] share one PostgreSQL container.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgresFixture>;
```

## Usage in Test Classes

```csharp
[Collection("Database")]
public class CategoryRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _db;

    public CategoryRepositoryTests(PostgresFixture db)
    {
        _db = db;
    }

    public async ValueTask InitializeAsync()
    {
        await _db.ResetAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task GetBySlugAsync_CaseInsensitive_ShouldFindCategory()
    {
        // This test hits real PostgreSQL with ILike — no longer skipped
    }
}
```

## How Respawn Works

Respawn introspects the database schema and generates `TRUNCATE ... CASCADE` statements for all user tables. It:

1. Preserves the schema (tables, indexes, constraints)
2. Removes all rows from all tables
3. Resets identity sequences
4. Respects FK relationships via CASCADE

This is orders of magnitude faster than `EnsureDeleted()` + `EnsureCreated()` or re-running migrations.

### Tables to Exclude (Optional)

If seed data should persist across tests (e.g., content types, core roles), configure Respawn to skip those tables:

```csharp
_respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
{
    DbAdapter = DbAdapter.Postgres,
    SchemasToInclude =
    [
        IdentityConstants.SchemaName,
        CoreConstants.SchemaName,
        ContentConstants.SchemaName,
    ],
    TablesToIgnore = [
        new Respawn.Graph.Table(ContentConstants.SchemaName, "content_types"),
    ],
});
```

## Container Lifecycle

```
Test Run Start
  └─ PostgresFixture.InitializeAsync()
       ├── Docker: Start postgres:16-alpine container
       ├── EF Core: Migrate identity schema
       ├── EF Core: Migrate core schema
       ├── EF Core: Migrate content schema
       └── Respawn: Create respawner with schema introspection

  Test Class 1
    ├── ResetAsync() — TRUNCATE all tables
    ├── Test 1a — insert data, assert, data stays
    ├── Test 1b — insert data, assert, data stays
    └── (data from 1a+1b still in DB)

  Test Class 2
    ├── ResetAsync() — TRUNCATE all tables (clean slate)
    ├── Test 2a — insert data, assert
    └── Test 2b — insert data, assert

Test Run End
  └─ PostgresFixture.DisposeAsync()
       └── Docker: Stop + remove container
```

## Troubleshooting

### Container Fails to Start

- Ensure Docker Desktop is running
- Check if port 5432 is available (the container uses a random port, but Docker daemon must be reachable)
- On CI: ensure Docker-in-Docker or a Docker service is configured

### Migrations Fail

- Ensure all three DbContexts have parameterless constructors or accept `DbContextOptions<T>`
- Check that `UseSnakeCaseNamingConvention()` is applied (must match production)

### Respawn Reset is Slow

- This usually means many tables with complex FK chains. The content schema has 29 tables — Respawn handles this efficiently but the first reset may be slower as it builds the dependency graph.
