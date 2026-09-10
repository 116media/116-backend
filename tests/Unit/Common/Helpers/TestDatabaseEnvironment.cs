namespace _116.Unit.Tests.Common.Helpers;

/// <summary>
/// Sets the Postgres variables registration code reads, restoring the previous values on
/// dispose. A test that builds a connection string needs one that parses; nothing here connects
/// to a database.
/// </summary>
public sealed class TestDatabaseEnvironment : IDisposable
{
    private static readonly (string Name, string Value)[] Variables =
    [
        ("POSTGRES_HOST", "localhost"),
        ("POSTGRES_PORT", "5432"),
        ("POSTGRES_DB", "unit_tests"),
        ("POSTGRES_USER", "unit_tests"),
        ("POSTGRES_PASSWORD", "unit_tests"),
    ];

    private readonly Dictionary<string, string?> _previous = [];

    /// <summary>
    /// Applies the test values, remembering whatever was set before.
    /// </summary>
    public TestDatabaseEnvironment()
    {
        foreach ((string name, string value) in Variables)
        {
            _previous[name] = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach ((string name, string? value) in _previous)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
