using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Handles the <see cref="PublicGetPopularArticlesQuery" /> to retrieve the most popular
/// published articles ranked by a weighted engagement score.
/// </summary>
/// <remarks>
/// Results are cached in-process for <see cref="CacheTtl" /> to avoid running the ranking
/// query on every request. Each distinct combination of limit, category, and excluded id
/// produces its own cache entry (e.g. <c>popular_articles_5_all_none</c>). All entries
/// share the eviction token supplied by <see cref="IPopularArticlesCacheInvalidator" />,
/// so any engagement or publish-state mutation cancels the token and evicts every entry
/// at once.
/// </remarks>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
/// <param name="cache">In-process memory cache.</param>
/// <param name="cacheInvalidator">Token source used to evict all popular-articles entries on mutation.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPopularArticlesHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMemoryCache cache,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    IMapper mapper
) : IQueryHandler<PublicGetPopularArticlesQuery, PublicGetPopularArticlesResult>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public async Task<PublicGetPopularArticlesResult> Handle(
        PublicGetPopularArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        string categoryPart = query.CategoryId?.ToString() ?? "all";
        string excludePart = query.ExcludeId?.ToString() ?? "none";
        string cacheKey = $"popular_articles_{query.Limit}_{categoryPart}_{excludePart}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<ArticleSummaryDto>? cached) && cached is not null)
        {
            return new PublicGetPopularArticlesResult(Articles: cached);
        }

        IReadOnlyList<ArticleEntity> articles = await articleRepository.GetPopularArticlesAsync(
            limit: query.Limit,
            excludeId: query.ExcludeId,
            categoryId: query.CategoryId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleSummaryDto> dtoList = await articles.ToArticleSummaryDtosAsync(
            mapper,
            fileRepository,
            cancellationToken
        );

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheTtl)
            .AddExpirationToken(new CancellationChangeToken(cacheInvalidator.GetEvictionToken()));

        cache.Set(cacheKey, dtoList, options);

        return new PublicGetPopularArticlesResult(Articles: dtoList);
    }
}
