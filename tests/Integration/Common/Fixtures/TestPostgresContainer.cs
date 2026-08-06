using _116.Content.Infrastructure.Persistence;
using _116.Core.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Owns the single PostgreSQL container the whole integration assembly runs against, and hands
/// every fixture a private database copied from one migrated template. Four collections
/// therefore cost one container start and one migration pass rather than four of each.
/// </summary>
internal static class TestPostgresContainer
{
    /// <summary>
    /// The database the module migrations are applied to. It is never used for test work, so
    /// that <c>CREATE DATABASE ... TEMPLATE</c> always finds it free of sessions.
    /// </summary>
    private const string TemplateDatabase = "test_116_template";

    /// <summary>
    /// The always-present maintenance database that <c>CREATE DATABASE</c> is issued from,
    /// since the statement cannot run from inside the database being copied.
    /// </summary>
    private const string MaintenanceDatabase = "postgres";

    /// <summary>
    /// PostgreSQL's <c>object_in_use</c> SQLSTATE, raised when the template still has a session
    /// attached at the moment a copy is attempted.
    /// </summary>
    private const string ObjectInUseSqlState = "55006";

    private const int CloneAttempts = 3;

    private static readonly SemaphoreSlim Gate = new(1, 1);

    /// <summary>
    /// The container backing every collection in the assembly. The data directory is a tmpfs
    /// mount because it is discarded when the run ends, which removes the filesystem work the
    /// per-test Respawn truncation spends most of its time in. The connection ceiling is raised
    /// because one server now backs four databases instead of four servers backing one each.
    /// </summary>
    private static readonly PostgreSqlContainer Container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase(TemplateDatabase)
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithTmpfsMount("/var/lib/postgresql/data")
        .WithCommand("-c", "max_connections=200")
        .Build();

    private static bool _templateReady;

    /// <summary>
    /// Starts the container and migrates the template on the first call, then creates
    /// <paramref name="database" /> as a copy of that template.
    /// </summary>
    /// <param name="database">The database name to create, unique per fixture type.</param>
    /// <returns>The connection string addressing the newly created database.</returns>
    public static async Task<string> LeaseDatabaseAsync(string database)
    {
        await Gate.WaitAsync();

        try
        {
            if (!_templateReady)
            {
                await Container.StartAsync();
                await MigrateTemplateAsync();
                NpgsqlConnection.ClearAllPools();
                _templateReady = true;
            }

            await CloneTemplateAsync(database);
        }
        finally
        {
            Gate.Release();
        }

        return ConnectionStringFor(database);
    }

    /// <summary>
    /// Closes the pooled connections a finished fixture left open, so its databases do not hold
    /// backends against the shared server for the rest of the run.
    /// </summary>
    /// <param name="connectionString">The connection string the fixture was leased.</param>
    public static void ReleaseDatabase(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return;
        }

        using var connection = new NpgsqlConnection(connectionString);
        NpgsqlConnection.ClearPool(connection);
    }

    /// <summary>
    /// Stops and removes the container once every collection in the assembly has finished.
    /// </summary>
    public static async ValueTask ShutdownAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await Container.DisposeAsync();
    }

    /// <summary>
    /// Copies the migrated template into a new database, retrying briefly if a session is still
    /// detaching from the template.
    /// </summary>
    /// <param name="database">The database name to create.</param>
    private static async Task CloneTemplateAsync(string database)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await ExecuteCloneAsync(database);
                return;
            }
            catch (PostgresException exception)
                when (exception.SqlState == ObjectInUseSqlState && attempt < CloneAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }
        }
    }

    /// <summary>
    /// Detaches any lingering session from the template and issues the copy.
    /// </summary>
    /// <param name="database">The database name to create.</param>
    private static async Task ExecuteCloneAsync(string database)
    {
        await using var connection = new NpgsqlConnection(ConnectionStringFor(MaintenanceDatabase));
        await connection.OpenAsync();

        await using (NpgsqlCommand terminate = connection.CreateCommand())
        {
            terminate.CommandText = """
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = @template AND pid <> pg_backend_pid();
                """;
            terminate.Parameters.AddWithValue("template", TemplateDatabase);
            await terminate.ExecuteNonQueryAsync();
        }

        await using NpgsqlCommand create = connection.CreateCommand();
        create.CommandText = $"""CREATE DATABASE "{database}" TEMPLATE "{TemplateDatabase}";""";
        await create.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Applies EF Core migrations for all four module DbContexts to the template database.
    /// </summary>
    private static async Task MigrateTemplateAsync()
    {
        string connectionString = ConnectionStringFor(TemplateDatabase);

        var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var identityContext = new IdentityDbContext(identityOptions);
        await identityContext.Database.MigrateAsync();

        var coreOptions = new DbContextOptionsBuilder<CoreDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var coreContext = new CoreDbContext(coreOptions);
        await coreContext.Database.MigrateAsync();

        var contentOptions = new DbContextOptionsBuilder<ContentDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var contentContext = new ContentDbContext(contentOptions);
        await contentContext.Database.MigrateAsync();

        var mailerOptions = new DbContextOptionsBuilder<MailerDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var mailerContext = new MailerDbContext(mailerOptions);
        await mailerContext.Database.MigrateAsync();
    }

    /// <summary>
    /// Rewrites the container's connection string to address a named database on it.
    /// </summary>
    /// <param name="database">The database to address.</param>
    /// <returns>The connection string for that database.</returns>
    private static string ConnectionStringFor(string database) =>
        new NpgsqlConnectionStringBuilder(Container.GetConnectionString()) { Database = database }.ConnectionString;
}
