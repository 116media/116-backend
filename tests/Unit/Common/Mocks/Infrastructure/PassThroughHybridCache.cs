using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Unit.Tests.Common.Mocks.Infrastructure;

/// <summary>
/// A <see cref="HybridCache"/> that stores nothing: every read runs the factory and every write
/// is discarded. Lets a unit test exercise a read-through code path without a cache hiding the
/// behaviour under test, and without a DI container.
/// </summary>
public sealed class PassThroughHybridCache : HybridCache
{
    /// <summary>
    /// Keys whose entries were written, in call order.
    /// </summary>
    public List<string> WrittenKeys { get; } = [];

    /// <summary>
    /// Tags evicted, in call order.
    /// </summary>
    public List<string> EvictedTags { get; } = [];

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
        return factory(state, cancellationToken);
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
        WrittenKeys.Add(key);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        EvictedTags.Add(tag);

        return ValueTask.CompletedTask;
    }
}
