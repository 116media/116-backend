using Testcontainers.Redis;

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Owns the single Redis container the whole integration assembly caches into. The host connects
/// eagerly when <c>REDIS_URL</c> is set, so pointing it at anything the machine may not be
/// running fails every cached handler; a container of our own is always there.
/// </summary>
internal static class TestRedisContainer
{
    private static readonly RedisContainer Container = new RedisBuilder("redis:7-alpine").Build();

    private static readonly SemaphoreSlim Gate = new(1, 1);

    private static string _connectionString = string.Empty;

    /// <summary>
    /// Starts the container on the first call and returns what the host should connect to. Admin
    /// mode is on so the suite can flush between tests.
    /// </summary>
    /// <returns>The connection string the test host caches into.</returns>
    public static async Task<string> ConnectionStringAsync()
    {
        await Gate.WaitAsync();

        try
        {
            if (_connectionString.Length == 0)
            {
                await Container.StartAsync();
                _connectionString = $"{Container.GetConnectionString()},allowAdmin=true";
            }
        }
        finally
        {
            Gate.Release();
        }

        return _connectionString;
    }

    /// <summary>
    /// Stops and removes the container once every collection has finished.
    /// </summary>
    public static ValueTask ShutdownAsync() => Container.DisposeAsync();
}
