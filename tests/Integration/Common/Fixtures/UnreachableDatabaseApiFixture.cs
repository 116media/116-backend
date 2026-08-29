namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> whose <c>POSTGRES_PORT</c> points at a closed port, so the
/// readiness probe observes an unreachable database while the host itself boots normally —
/// module contexts are rewired to the real Testcontainer and Testing hosts run no seeders.
/// </summary>
/// <param name="db">The Testcontainer database backing this host.</param>
public class UnreachableDatabaseApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <summary>
    /// The environment variable the readiness probe's connection string reads the port from.
    /// </summary>
    private const string PostgresPortVariable = "POSTGRES_PORT";

    private string? _previousPort;

    /// <inheritdoc />
    protected override void ConfigureEnvironment()
    {
        base.ConfigureEnvironment();

        _previousPort = Environment.GetEnvironmentVariable(PostgresPortVariable);
        Environment.SetEnvironmentVariable(PostgresPortVariable, "1");
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable(PostgresPortVariable, _previousPort);

        base.Dispose(disposing);
    }
}
