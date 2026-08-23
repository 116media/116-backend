using Npgsql;
using Respawn;

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Provides a migrated PostgreSQL database for an integration test collection.
/// The database is leased from the assembly-wide container in
/// <see cref="TestPostgresContainer" />, exposes a Respawn-based reset for per-test cleanup,
/// and creates a shared <see cref="ApiFixture" /> so the application is booted exactly once.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private Respawner? _respawner;
    private ApiFixture? _apiFixture;
    private string _connectionString = string.Empty;

    /// <summary>
    /// The connection string to this fixture's database on the shared Testcontainer.
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// The shared API fixture (WebApplicationFactory) for this collection.
    /// Created once, reused by every test class.
    /// </summary>
    public ApiFixture Api => _apiFixture ?? throw new InvalidOperationException("PostgresFixture not initialized");

    /// <summary>
    /// The database this fixture leases. Derived from the fixture type so that every fixture
    /// gets its own database on the shared container without a name having to be maintained.
    /// </summary>
    protected virtual string DatabaseName => $"test_116_{GetType().Name.ToLowerInvariant()}";

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        _connectionString = await TestPostgresContainer.LeaseDatabaseAsync(DatabaseName);
        await CreateRespawnerAsync();

        _apiFixture = CreateApiFixture();
        _ = _apiFixture.Services;
    }

    /// <summary>
    /// Creates the <see cref="ApiFixture" /> that boots the application for this database.
    /// Derived fixtures override this to substitute a differently configured host, such as one
    /// that keeps the production rate limit policies active.
    /// </summary>
    /// <returns>The API fixture used for the lifetime of the collection.</returns>
    protected virtual ApiFixture CreateApiFixture() => new(this);

    /// <summary>
    /// Truncates all data across the identity, core, content, and mailer schemas.
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
    public ValueTask DisposeAsync()
    {
        _apiFixture?.Dispose();
        TestPostgresContainer.ReleaseDatabase(_connectionString);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Creates a Respawner targeting the four module schemas of this fixture's database.
    /// </summary>
    private async Task CreateRespawnerAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["identity", "core", "content", "mailer"],
            }
        );
    }
}
