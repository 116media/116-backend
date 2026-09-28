using System.Net.Sockets;
using _116.Content.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Persistence;
using _116.Storage.Infrastructure.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;

namespace _116.Tests.Fixtures;

/// <summary>
/// Owns the single PostgreSQL container every integration assembly runs against, and hands each
/// fixture a private database copied from one migrated template. The container is reused across
/// processes, so the six test assemblies cost one container start and one migration pass between
/// them rather than one of each.
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
    /// Table written into the template once every migration has been applied. The template database
    /// itself is created empty by the container, so its existence proves nothing.
    /// </summary>
    private const string TemplateReadyTable = "template_ready";

    /// <summary>
    /// PostgreSQL's <c>object_in_use</c> SQLSTATE, raised when the template still has a session
    /// attached at the moment a copy is attempted.
    /// </summary>
    private const string ObjectInUseSqlState = "55006";

    private const int CloneAttempts = 3;

    /// <summary>
    /// Seconds to wait for another process's container to accept connections after losing the start race.
    /// </summary>
    private const int StartRaceAttempts = 60;

    /// <summary>
    /// Key for the session-level advisory lock that serializes template migration and cloning across
    /// processes. An in-process semaphore cannot: with a shared container the racing parties are
    /// separate test assemblies, each in its own process.
    /// </summary>
    private const long TemplateLockKey = 116_000_001L;

    private const string Username = "test_user";

    private const string Password = "test_password";

    /// <summary>
    /// Name and host port of the container every assembly shares. Fixed rather than random because that is
    /// what lets a second process find the first one's server: Testcontainers' own reuse attaches to the
    /// container but then times out re-running its readiness checks against it, so the coordination is ours.
    /// </summary>
    private const string ContainerName = "116_integration_postgres";

    private static readonly int SharedPort = int.TryParse(
        Environment.GetEnvironmentVariable("_116_TESTS_POSTGRES_PORT"),
        out int configured
    )
        ? configured
        : 54_329;

    /// <summary>
    /// Set <c>_116_TESTS_NO_CONTAINER_REUSE=1</c> to get one container per assembly on a random port, removed
    /// when the assembly finishes. Sharing is the default because it is what makes six assemblies cheap; the
    /// cost is a container left running between runs, which the name and label make easy to reap.
    /// </summary>
    private static readonly bool ShareContainer =
        Environment.GetEnvironmentVariable("_116_TESTS_NO_CONTAINER_REUSE") is not "1";

    private static readonly SemaphoreSlim StartGate = new(1, 1);

    private static readonly HashSet<string> LeasedDatabases = [];

    /// <summary>
    /// The container backing every collection in every assembly. The data directory is a tmpfs
    /// mount because it is discarded when the run ends, which removes the filesystem work the
    /// per-test Respawn truncation spends most of its time in. The connection ceiling is raised
    /// because one server now backs every fixture of every assembly.
    /// </summary>
    private static readonly PostgreSqlContainer Container = BuildContainer();

    private static bool _started;

    /// <summary>
    /// Builds the container definition: pinned name and port when it is shared, Testcontainers' defaults when
    /// each assembly gets its own.
    /// </summary>
    private static PostgreSqlContainer BuildContainer()
    {
        PostgreSqlBuilder builder = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase(TemplateDatabase)
            .WithUsername(Username)
            .WithPassword(Password)
            .WithTmpfsMount("/var/lib/postgresql/data")
            .WithCommand("-c", "max_connections=400")
            .WithLabel("116.integration", "postgres");

        // Cleanup off: the resource reaper deletes a container when the process that created it exits, which
        // would take the server away from the assemblies still to run. The fixed name and label are how it is
        // found again, and how it is removed when someone wants it gone.
        return ShareContainer
            ? builder.WithName(ContainerName).WithPortBinding(SharedPort, 5432).WithCleanUp(false).Build()
            : builder.Build();
    }

    /// <summary>
    /// Starts (or attaches to) the container, migrates the template if no process has yet, then
    /// creates <paramref name="database" /> as a copy of that template.
    /// </summary>
    /// <param name="database">The database name to create, unique per fixture and per assembly.</param>
    /// <returns>The connection string addressing the newly created database.</returns>
    public static async Task<string> LeaseDatabaseAsync(string database)
    {
        await EnsureStartedAsync();

        await using var maintenance = new NpgsqlConnection(ConnectionStringFor(MaintenanceDatabase));
        await maintenance.OpenAsync();
        await ExecuteAsync(maintenance, $"SELECT pg_advisory_lock({TemplateLockKey});");

        try
        {
            if (!await TemplateIsReadyAsync())
            {
                await MigrateTemplateAsync();
                await MarkTemplateReadyAsync();
                NpgsqlConnection.ClearAllPools();
            }

            await CloneTemplateAsync(maintenance, database);
        }
        finally
        {
            await ExecuteAsync(maintenance, $"SELECT pg_advisory_unlock({TemplateLockKey});");
        }

        lock (LeasedDatabases)
        {
            LeasedDatabases.Add(database);
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
    /// Drops the databases this assembly leased once its collections have finished, and removes the
    /// container unless it is being reused by the other assemblies.
    /// </summary>
    public static async ValueTask ShutdownAsync()
    {
        NpgsqlConnection.ClearAllPools();

        if (!_started)
        {
            return;
        }

        await DropLeasedDatabasesAsync();

        if (!ShareContainer)
        {
            await Container.DisposeAsync();
        }
    }

    /// <summary>
    /// Makes sure a server is up, once per process: joins the shared one if another assembly already started
    /// it, otherwise starts it. Losing the race is expected — both processes then use the same server.
    /// </summary>
    private static async Task EnsureStartedAsync()
    {
        if (_started)
        {
            return;
        }

        await StartGate.WaitAsync();

        try
        {
            if (_started)
            {
                return;
            }

            if (ShareContainer && await ServerRespondsAsync())
            {
                _started = true;
                return;
            }

            try
            {
                await Container.StartAsync();
            }
            catch when (ShareContainer)
            {
                await WaitForServerAsync();
            }

            _started = true;
        }
        finally
        {
            StartGate.Release();
        }
    }

    /// <summary>
    /// Reports whether the shared server already accepts connections.
    /// </summary>
    /// <returns>True when a server answers on the shared port.</returns>
    private static async Task<bool> ServerRespondsAsync()
    {
        try
        {
            await using var probe = new NpgsqlConnection(ConnectionStringFor(MaintenanceDatabase));
            await probe.OpenAsync();
            return true;
        }
        catch (NpgsqlException)
        {
            return false;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for the process that won the start race to finish bringing the server up.
    /// </summary>
    private static async Task WaitForServerAsync()
    {
        for (var attempt = 1; attempt <= StartRaceAttempts; attempt++)
        {
            if (await ServerRespondsAsync())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new InvalidOperationException(
            $"No PostgreSQL server on port {SharedPort} after {StartRaceAttempts}s, and this process could not start one."
        );
    }

    /// <summary>
    /// Reports whether some process has already migrated the template, by looking for the marker
    /// table. The connection is closed before returning so it cannot block a later copy.
    /// </summary>
    /// <returns>True when the template carries every migration.</returns>
    private static async Task<bool> TemplateIsReadyAsync()
    {
        string connectionString = ConnectionStringFor(TemplateDatabase);

        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await using NpgsqlCommand marker = connection.CreateCommand();
            marker.CommandText = $"SELECT to_regclass('public.{TemplateReadyTable}') IS NOT NULL;";
            var ready = (bool)(await marker.ExecuteScalarAsync())!;

            if (!ready)
            {
                return false;
            }
        }

        using var pooled = new NpgsqlConnection(connectionString);
        NpgsqlConnection.ClearPool(pooled);
        return true;
    }

    /// <summary>
    /// Writes the marker table that tells every other process the template is fully migrated.
    /// </summary>
    private static async Task MarkTemplateReadyAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionStringFor(TemplateDatabase));
        await connection.OpenAsync();
        await ExecuteAsync(connection, $"CREATE TABLE IF NOT EXISTS public.{TemplateReadyTable} ();");
    }

    /// <summary>
    /// Copies the migrated template into a new database, retrying briefly if a session is still
    /// detaching from the template.
    /// </summary>
    /// <param name="maintenance">An open connection to the maintenance database.</param>
    /// <param name="database">The database name to create.</param>
    private static async Task CloneTemplateAsync(NpgsqlConnection maintenance, string database)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await ExecuteCloneAsync(maintenance, database);
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
    /// Detaches any lingering session from the template and issues the copy. The drop first is what
    /// lets a reused container serve a second run: the database this fixture leases may still be
    /// there from the last one.
    /// </summary>
    /// <param name="maintenance">An open connection to the maintenance database.</param>
    /// <param name="database">The database name to create.</param>
    private static async Task ExecuteCloneAsync(NpgsqlConnection maintenance, string database)
    {
        await using (NpgsqlCommand terminate = maintenance.CreateCommand())
        {
            terminate.CommandText = """
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = @template AND pid <> pg_backend_pid();
                """;
            terminate.Parameters.AddWithValue("template", TemplateDatabase);
            await terminate.ExecuteNonQueryAsync();
        }

        await ExecuteAsync(maintenance, $"""DROP DATABASE IF EXISTS "{database}" WITH (FORCE);""");
        await ExecuteAsync(maintenance, $"""CREATE DATABASE "{database}" TEMPLATE "{TemplateDatabase}";""");
    }

    /// <summary>
    /// Drops every database this process leased, so a reused container does not accumulate them.
    /// </summary>
    private static async Task DropLeasedDatabasesAsync()
    {
        string[] databases;

        lock (LeasedDatabases)
        {
            databases = [.. LeasedDatabases];
            LeasedDatabases.Clear();
        }

        if (databases.Length == 0)
        {
            return;
        }

        await using var maintenance = new NpgsqlConnection(ConnectionStringFor(MaintenanceDatabase));
        await maintenance.OpenAsync();

        foreach (string database in databases)
        {
            await ExecuteAsync(maintenance, $"""DROP DATABASE IF EXISTS "{database}" WITH (FORCE);""");
        }
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

        var storageOptions = new DbContextOptionsBuilder<StorageDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        await using var storageContext = new StorageDbContext(storageOptions);
        await storageContext.Database.MigrateAsync();

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
    /// Runs a statement that returns nothing the caller needs.
    /// </summary>
    /// <param name="connection">An open connection.</param>
    /// <param name="sql">The statement to run.</param>
    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Rewrites the container's connection string to address a named database on it.
    /// </summary>
    /// <param name="database">The database to address.</param>
    /// <returns>The connection string for that database.</returns>
    private static string ConnectionStringFor(string database)
    {
        // The shared server is addressed by its pinned port rather than by the container handle, which only the
        // process that started it can ask for a mapping.
        NpgsqlConnectionStringBuilder builder = ShareContainer
            ? new NpgsqlConnectionStringBuilder
            {
                Host = "127.0.0.1",
                Port = SharedPort,
                Username = Username,
                Password = Password,
            }
            : new NpgsqlConnectionStringBuilder(Container.GetConnectionString());

        builder.Database = database;
        return builder.ConnectionString;
    }
}
