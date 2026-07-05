using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Handles the <see cref="PublicGetAllTagsQuery" /> to retrieve all tags.
/// </summary>
/// <remarks>
/// Only unfiltered results — those requested with a blank <c>Search</c> term — are cached
/// in-process for <see cref="CacheTtl" />. Each such entry is keyed by limit and content type
/// (e.g. <c>all_tags_all_null</c>, <c>all_tags_50_article</c>, <c>all_tags_10_video</c>), so the
/// distinct unfiltered variants cache independently.
/// <para>
/// Requests that carry a search term bypass the cache entirely and query the repository on
/// every call. Free-text search would otherwise produce an unbounded number of distinct
/// cache keys, so those results are never stored.
/// </para>
/// <para>
/// Cached entries register the eviction token supplied by <see cref="IPopularTagsCacheInvalidator" />
/// — reused from the popular-tags caching path — as an expiration token. Any tag CRUD operation
/// or article/video tag-association change cancels the shared token and instantly evicts every
/// cached all-tags entry alongside the popular-tags entries.
/// </para>
/// </remarks>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="cache">In-process memory cache.</param>
/// <param name="cacheInvalidator">Token source used to evict all cached all-tags entries on mutation.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetAllTagsHandler(
    ILookupRepository lookupRepository,
    IMemoryCache cache,
    IPopularTagsCacheInvalidator cacheInvalidator,
    IMapper mapper
) : IQueryHandler<PublicGetAllTagsQuery, PublicGetAllTagsResult>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public async Task<PublicGetAllTagsResult> Handle(PublicGetAllTagsQuery query, CancellationToken cancellationToken)
    {
        bool cacheable = string.IsNullOrWhiteSpace(query.Search);
        string limitPart = query.Limit?.ToString() ?? "all";
        string contentTypePart = query.ContentType?.ToString().ToLowerInvariant() ?? "null";
        string cacheKey = $"all_tags_{limitPart}_{contentTypePart}";

        if (cacheable && cache.TryGetValue(cacheKey, out IReadOnlyList<TagDto>? cached) && cached is not null)
        {
            return new PublicGetAllTagsResult(Tags: cached);
        }

        IReadOnlyList<TagEntity> tags = await lookupRepository.GetAllTagsAsync(
            search: query.Search,
            contentType: query.ContentType,
            limit: query.Limit,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<TagDto> dtoList = tags.ToTagDtos(mapper);

        if (cacheable)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheTtl)
                .AddExpirationToken(new CancellationChangeToken(cacheInvalidator.GetEvictionToken()));

            cache.Set(cacheKey, dtoList, options);
        }

        return new PublicGetAllTagsResult(Tags: dtoList);
    }
}
