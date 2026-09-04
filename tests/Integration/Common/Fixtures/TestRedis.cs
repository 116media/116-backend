namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// The Redis the test host caches into: the locally published instance, on a database index of
/// its own so a run never reads or flushes the developer's cache.
/// </summary>
public static class TestRedis
{
    /// <summary>
    /// The database index reserved for tests.
    /// </summary>
    public const int DatabaseIndex = 15;

    /// <summary>
    /// The connection string the host is pointed at. Admin mode is on so the suite can flush
    /// its own database between tests.
    /// </summary>
    public static readonly string ConnectionString =
        $"localhost:6379,defaultDatabase={DatabaseIndex},allowAdmin=true,abortConnect=false";
}
