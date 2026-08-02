using _116.Content.Infrastructure.Persistence;
using _116.Core.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Manages a PostgreSQL Testcontainer and Respawn for database reset between tests.
/// </summary>
/// <summary>
/// Provides a shared PostgreSQL container for integration tests.
/// Starts a postgres:16-alpine container, applies EF migrations for all modules,
/// exposes a Respawn-based reset for per-test cleanup, and creates a shared
/// <see cref="ApiFixture" /> so the application is booted exactly once.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test_116_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    private Respawner? _respawner;
    private ApiFixture? _apiFixture;

    /// <summary>
    /// The connection string to the running Testcontainer database.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// The shared API fixture (WebApplicationFactory) for the entire test run.
    /// Created once, reused by every test class.
    /// </summary>
    public ApiFixture Api => _apiFixture ?? throw new InvalidOperationException("PostgresFixture not initialized");

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        await ApplyMigrationsAsync();
        await CreateRespawnerAsync();

        _apiFixture = CreateApiFixture();
        _ = _apiFixture.Services;
    }

    /// <summary>
    /// Creates the <see cref="ApiFixture" /> that boots the application for this container.
    /// Derived fixtures override this to substitute a differently configured host, such as one
    /// that keeps the production rate limit policies active.
    /// </summary>
    /// <returns>The API fixture used for the lifetime of the container.</returns>
    protected virtual ApiFixture CreateApiFixture() => new(this);

    /// <summary>
    /// Truncates all data across identity, core, and content schemas.
    /// </summary>
    public async Task ResetAsync()
    {
        if (_respawner is not null)
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_apiFixture is not null)
        {
            _apiFixture.Dispose();
        }

        await _container.DisposeAsync();
    }

    /// <summary>
    /// Applies EF Core migrations for all three module DbContexts.
    /// </summary>
    private async Task ApplyMigrationsAsync()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var identityContext = new IdentityDbContext(options);
        await identityContext.Database.MigrateAsync();

        var coreOptions = new DbContextOptionsBuilder<CoreDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var coreContext = new CoreDbContext(coreOptions);
        await coreContext.Database.MigrateAsync();

        var contentOptions = new DbContextOptionsBuilder<ContentDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var contentContext = new ContentDbContext(contentOptions);
        await contentContext.Database.MigrateAsync();
    }

    /// <summary>
    /// Creates a Respawner targeting all three module schemas.
    /// </summary>
    private async Task CreateRespawnerAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { DbAdapter = DbAdapter.Postgres, SchemasToInclude = ["identity", "core", "content"] }
        );
    }
}
