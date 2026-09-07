using _116.Shared.Infrastructure.Cache;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace _116.Integration.Tests.Shared.Infrastructure.Cache;

/// <summary>
/// Proves the hybrid cache's distributed layer across two independent cache instances sharing
/// one Redis — the configuration a multi-instance deployment runs. Instance boundaries are the
/// unit under test here, so each instance is a separate provider over the shared container
/// rather than a booted application host.
/// </summary>
public sealed class CacheCoherenceTests : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();

    private ServiceProvider _providerA = null!;
    private ServiceProvider _providerB = null!;
    private HybridCache _instanceA = null!;
    private HybridCache _instanceB = null!;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _redis.StartAsync();

        _providerA = BuildInstance(_redis.GetConnectionString());
        _providerB = BuildInstance(_redis.GetConnectionString());
        _instanceA = _providerA.GetRequiredService<HybridCache>();
        _instanceB = _providerB.GetRequiredService<HybridCache>();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _providerA.DisposeAsync();
        await _providerB.DisposeAsync();
        await _redis.DisposeAsync();
    }

    /// <summary>
    /// Waits until the shared distributed layer holds the key. The hybrid cache populates L2 in
    /// the background after serving the caller, so assertions against the second instance must
    /// not race that write.
    /// </summary>
    private async Task WaitForSharedEntryAsync(string key)
    {
        var distributed = _providerB.GetRequiredService<IDistributedCache>();

        for (int attempt = 0; attempt < 100; attempt++)
        {
            if (await distributed.GetAsync(key) is not null)
            {
                return;
            }

            await Task.Delay(20);
        }

        throw new TimeoutException($"Entry '{key}' never reached the shared cache layer.");
    }

    /// <summary>
    /// Builds one cache instance the way the application host does: its own L1, the shared
    /// Redis as L2, and the eviction backplane over the same Redis.
    /// </summary>
    private static ServiceProvider BuildInstance(string redisConnectionString)
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "116:";
        });
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddHybridCache();
        services.Decorate<HybridCache, BackplaneHybridCache>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task GetOrCreate_OnASecondInstance_ShouldServeTheEntryTheFirstInstanceStored()
    {
        // Arrange — instance A fills the shared L2
        string key = $"coherence:share:{Guid.NewGuid():N}";
        int factoryACalls = 0;
        int factoryBCalls = 0;

        await _instanceA.GetOrCreateAsync(
            key,
            _ =>
            {
                factoryACalls++;
                return ValueTask.FromResult("from-a");
            }
        );

        await WaitForSharedEntryAsync(key);

        // Act — instance B has a cold L1, so a hit proves it read the shared L2
        string result = await _instanceB.GetOrCreateAsync(
            key,
            _ =>
            {
                factoryBCalls++;
                return ValueTask.FromResult("from-b");
            }
        );

        // Assert
        result.Should().Be("from-a");
        factoryACalls.Should().Be(1);
        factoryBCalls.Should().Be(0);
    }

    [Fact]
    public async Task RemoveByTag_OnOneInstance_ShouldReachAWarmInstanceThroughTheBackplane()
    {
        // Arrange — instance A stores a tagged entry AND holds a warm local view of the tag,
        // which the hybrid cache never re-reads on its own. Without the backplane this test
        // fails: A would serve the entry until its TTL. This is the guarantee production
        // relies on — every instance subscribes at construction, so any later eviction
        // anywhere reaches every instance that could hold the entry.
        string key = $"coherence:converge:{Guid.NewGuid():N}";
        const string tag = "coherence:warm-tag";
        int factoryCalls = 0;

        async ValueTask<string> ReadThroughA()
        {
            return await _instanceA.GetOrCreateAsync(
                key,
                _ =>
                {
                    factoryCalls++;
                    return ValueTask.FromResult($"fill-{factoryCalls}");
                },
                tags: [tag]
            );
        }

        (await ReadThroughA()).Should().Be("fill-1");
        await WaitForSharedEntryAsync(key);
        (await ReadThroughA()).Should().Be("fill-1");

        // Act — the eviction happens on the OTHER instance
        await _instanceB.RemoveByTagAsync(tag);

        // Assert — the pub/sub delivery is asynchronous, so the assertion polls to a bound far
        // below the entry TTL the eviction would otherwise wait out
        for (int attempt = 0; attempt < 100; attempt++)
        {
            if (await ReadThroughA() == "fill-2")
            {
                return;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException("Instance A never observed the cross-instance tag invalidation.");
    }

    [Fact]
    public async Task RemoveByKey_OnOneInstance_ShouldEvictTheSharedEntryForTheOther()
    {
        // Arrange
        string key = $"coherence:remove:{Guid.NewGuid():N}";
        int factoryCalls = 0;

        async ValueTask<string> ReadThroughA()
        {
            return await _instanceA.GetOrCreateAsync(
                key,
                _ =>
                {
                    factoryCalls++;
                    return ValueTask.FromResult($"fill-{factoryCalls}");
                },
                new HybridCacheEntryOptions { Flags = HybridCacheEntryFlags.DisableLocalCacheRead }
            );
        }

        (await ReadThroughA()).Should().Be("fill-1");
        await WaitForSharedEntryAsync(key);

        // Act
        await _instanceB.RemoveAsync(key);

        // Assert
        (await ReadThroughA())
            .Should()
            .Be("fill-2");
    }
}
