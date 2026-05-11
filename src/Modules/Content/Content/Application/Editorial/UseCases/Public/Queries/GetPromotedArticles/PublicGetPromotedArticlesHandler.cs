using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;

/// <summary>
/// Handles the <see cref="PublicGetPromotedArticlesQuery" /> to retrieve all currently promoted published articles.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPromotedArticlesHandler(IArticleRepository articleRepository, IMapper mapper)
    : IQueryHandler<PublicGetPromotedArticlesQuery, PublicGetPromotedArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPromotedArticlesResult> Handle(
        PublicGetPromotedArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ArticleEntity> articles = await articleRepository.GetPromotedAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleSummaryDto> dtoList = articles.ToArticleSummaryDtos(mapper);

        return new PublicGetPromotedArticlesResult(Articles: dtoList);
    }
}
