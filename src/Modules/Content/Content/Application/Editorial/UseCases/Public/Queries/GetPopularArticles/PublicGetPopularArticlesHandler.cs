using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Handles the <see cref="PublicGetPopularArticlesQuery" /> to retrieve the most popular
/// published articles ranked by a weighted engagement score.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPopularArticlesHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMapper mapper
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

        IReadOnlyList<ArticleSummaryDto> dtoList = await articles.ToArticleSummaryDtosAsync(
            mapper,
            fileRepository,
            cancellationToken
        );

        return new PublicGetPopularArticlesResult(Articles: dtoList);
    }
}
