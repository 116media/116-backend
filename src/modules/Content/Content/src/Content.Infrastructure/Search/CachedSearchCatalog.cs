using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Content.Infrastructure.Search;

/// <summary>
/// Caches the per-scope hit counts in front of <see cref="ISearchCatalog" />. The counts are
/// the same for every page of a term but cost a scan of the match set, so each page would
/// otherwise re-count it. The ranked pages are not cached: they differ per page and the
/// endpoint answers <c>no-store</c>.
/// </summary>
/// <param name="inner">The catalog running the real queries.</param>
/// <param name="cache">The hybrid cache holding the counts.</param>
/// <param name="projectors">The registered projectors, read for the tags that evict a count.</param>
public class CachedSearchCatalog(
    ISearchCatalog inner,
    HybridCache cache,
    IEnumerable<ISearchDocumentProjector> projectors
) : ISearchCatalog
{
    /// <summary>
    /// How long a count stays valid absent an explicit eviction. Short, because a reader
    /// seeing a tab count lag a publish by under a minute costs nothing.
    /// </summary>
    private static readonly HybridCacheEntryOptions Options = new()
    {
        Expiration = TimeSpan.FromMinutes(1),
        LocalCacheExpiration = TimeSpan.FromMinutes(1),
    };

    /// <summary>
    /// Publishing or retiring any searchable type changes the totals, so every registered
    /// type's tag evicts the counts.
    /// </summary>
    private string[] Tags { get; } = [.. projectors.Select(projector => ContentCacheTags.For(projector.Type))];

    /// <inheritdoc />
    public Task<IReadOnlyList<SearchHitDto>> SearchAsync(
        EnumSearchScope scope,
        string query,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    ) => inner.SearchAsync(scope, query, skip, take, cancellationToken);

    /// <inheritdoc />
    public async Task<SearchCountsDto> CountAsync(string query, CancellationToken cancellationToken = default)
    {
        // The state overload keeps the factory static, so no closure is allocated per request.
        return await cache.GetOrCreateAsync(
            key: CacheKey(query),
            state: (inner, query),
            factory: static async (state, token) => await state.inner.CountAsync(state.query, token),
            options: Options,
            tags: Tags,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Builds the key for a term. The term is lowercased so the counts are shared across
    /// casings, which the full-text query treats alike anyway.
    /// </summary>
    /// <param name="query">The trimmed term.</param>
    /// <returns>The cache key.</returns>
    private static string CacheKey(string query) => $"search_counts:{query.ToLowerInvariant()}";
}
