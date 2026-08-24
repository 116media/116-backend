using Microsoft.Extensions.Caching.Hybrid;
using StackExchange.Redis;

namespace _116.Shared.Infrastructure.Cache;

/// <summary>
/// Decorator over <see cref="HybridCache" /> that broadcasts evictions on a Redis channel and
/// replays evictions broadcast by other instances, so a tag or key removed anywhere is removed
/// from every instance's local layer at once.
/// </summary>
/// <remarks>
/// The hybrid cache caches each tag's invalidation timestamp in-process and never re-reads it,
/// so without a backplane a warm instance keeps serving tagged entries until they expire on
/// their own TTL. Replaying the eviction locally is the supported way to refresh that view.
/// </remarks>
public sealed class BackplaneHybridCache : HybridCache
{
    private const string Channel = "116:cache-eviction";
    private const string TagPrefix = "t:";
    private const string KeyPrefix = "k:";

    private readonly HybridCache _inner;
    private readonly ISubscriber _subscriber;

    /// <summary>
    /// Wraps the inner cache and starts listening for evictions from other instances.
    /// </summary>
    /// <param name="inner">The decorated hybrid cache.</param>
    /// <param name="connection">The Redis connection the eviction channel rides on.</param>
    public BackplaneHybridCache(HybridCache inner, IConnectionMultiplexer connection)
    {
        _inner = inner;
        _subscriber = connection.GetSubscriber();

        _subscriber.Subscribe(
            RedisChannel.Literal(Channel),
            (channel, message) =>
            {
                string? payload = message;
                if (payload is null)
                {
                    return;
                }

                // Replay through the inner cache only: replaying through this instance would
                // re-broadcast and loop the message forever.
                if (payload.StartsWith(TagPrefix, StringComparison.Ordinal))
                {
                    ValueTask pending = _inner.RemoveByTagAsync(payload[TagPrefix.Length..]);
                    _ = pending.Preserve();
                }
                else if (payload.StartsWith(KeyPrefix, StringComparison.Ordinal))
                {
                    ValueTask pending = _inner.RemoveAsync(payload[KeyPrefix.Length..]);
                    _ = pending.Preserve();
                }
            }
        );
    }

    /// <inheritdoc />
    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default
    )
    {
        return _inner.GetOrCreateAsync(key, state, factory, options, tags, cancellationToken);
    }

    /// <inheritdoc />
    public override ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default
    )
    {
        return _inner.SetAsync(key, value, options, tags, cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(key, cancellationToken);
        await _subscriber.PublishAsync(RedisChannel.Literal(Channel), $"{KeyPrefix}{key}");
    }

    /// <inheritdoc />
    public override async ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveByTagAsync(tag, cancellationToken);
        await _subscriber.PublishAsync(RedisChannel.Literal(Channel), $"{TagPrefix}{tag}");
    }
}
