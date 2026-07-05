using _116.Content.Application.Shared.Cache;

namespace _116.Content.Infrastructure.Cache;

/// <summary>
/// Singleton implementation of <see cref="IPopularArticlesCacheInvalidator" /> that uses a
/// <see cref="CancellationTokenSource" /> as an eviction token for <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /> entries.
/// </summary>
/// <remarks>
/// Cache entries created by <see cref="Application.Editorial.UseCases.Public.Queries.GetPopularArticles.PublicGetPopularArticlesHandler" />
/// register the token returned by <see cref="GetEvictionToken" /> as a
/// <see cref="Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions.ExpirationTokens">expiration token</see>.
/// Calling <see cref="Invalidate" /> cancels the source, which causes the memory cache to
/// evict all registered entries immediately — regardless of the limit, category, or
/// exclude-id used when those entries were stored. A new <see cref="CancellationTokenSource" />
/// is then created so subsequent cache fills are not affected.
/// All mutations are protected by a lock so the class is safe to use as a singleton.
/// </remarks>
public sealed class PopularArticlesCacheInvalidator : IPopularArticlesCacheInvalidator
{
    private readonly Lock _lock = new();
    private CancellationTokenSource _cts = new();

    /// <inheritdoc />
    public CancellationToken GetEvictionToken()
    {
        lock (_lock)
        {
            return _cts.Token;
        }
    }

    /// <inheritdoc />
    public void Invalidate()
    {
        CancellationTokenSource old;

        lock (_lock)
        {
            old = _cts;
            _cts = new CancellationTokenSource();
        }

        old.Cancel();
        old.Dispose();
    }
}
