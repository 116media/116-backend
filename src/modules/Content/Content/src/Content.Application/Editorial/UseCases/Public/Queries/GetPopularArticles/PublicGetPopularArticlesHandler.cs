using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Storage.Contracts.Application.Services;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Handles the <see cref="PublicGetPopularArticlesQuery" /> to retrieve the most popular
/// published articles ranked by a weighted engagement score.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="fileStorage">Storage's file contract.</param>
public class PublicGetPopularArticlesHandler(
    IArticleRepository articleRepository,
    IFileStorageService fileStorage,
    IContentLookupService contentLookupService
) : IQueryHandler<PublicGetPopularArticlesQuery, PublicGetPopularArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPopularArticlesResult> Handle(
        PublicGetPopularArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ArticleEntity> articles = await articleRepository.GetPopularArticlesAsync(
            limit: query.Limit,
            excludeId: query.ExcludeId,
            categoryId: query.CategoryId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicArticleSummaryDto> dtoList = await articles.ToPublicArticleSummaryDtosAsync(
            await contentLookupService.ResolveForArticlesAsync(articles, cancellationToken),
            fileStorage,
            cancellationToken
        );

        return new PublicGetPopularArticlesResult(Articles: dtoList);
    }
}
