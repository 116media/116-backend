using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.Extensions.Caching.Memory;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags;

/// <summary>
/// Handles the <see cref="PublicGetPopularTagsQuery" /> to retrieve the most-used tags.
/// </summary>
/// <remarks>
/// Results are cached in-process for <see cref="CacheTtl" /> to avoid running the
/// GROUP BY aggregation query on every request. Popular tags change infrequently,
/// so a short TTL is acceptable and keeps the cache warm under normal traffic.
/// <para>
/// Each distinct <paramref name="query" /> limit value produces an independent cache
/// entry (e.g. <c>popular_tags_8</c>, <c>popular_tags_null</c>). All entries share the
/// same eviction token supplied by <see cref="IPopularTagsCacheInvalidator" />, so any
/// tag-graph mutation — adding or removing article / video tag associations — cancels
/// the token and instantly evicts every entry regardless of which limits were cached.
/// </para>
/// </remarks>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="cache">In-process memory cache.</param>
/// <param name="cacheInvalidator">Token source used to evict all popular-tags entries on mutation.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPopularTagsHandler(
    ILookupRepository lookupRepository,
    IMemoryCache cache,
    IPopularTagsCacheInvalidator cacheInvalidator,
    IMapper mapper
) : IQueryHandler<PublicGetPopularTagsQuery, PublicGetPopularTagsResult>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public async Task<PublicGetPopularTagsResult> Handle(
        PublicGetPopularTagsQuery query,
        CancellationToken cancellationToken
    )
    {
        string cacheKey = $"popular_tags_{query.Limit?.ToString() ?? "all"}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<TagDto>? cached) && cached is not null)
        {
            return new PublicGetPopularTagsResult(Tags: cached);
        }

        IReadOnlyList<TagEntity> tags = await lookupRepository.GetPopularTagsAsync(
            limit: query.Limit,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<TagDto> dtoList = tags.ToTagDtos(mapper);

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheTtl)
            .AddExpirationToken(
                new Microsoft.Extensions.Primitives.CancellationChangeToken(cacheInvalidator.GetEvictionToken())
            );

        cache.Set(cacheKey, dtoList, options);

        return new PublicGetPopularTagsResult(Tags: dtoList);
    }
}
