using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles;

/// <summary>
/// Handles the <see cref="GetFeaturedArticlesQuery" /> to retrieve all currently featured published articles.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetFeaturedArticlesHandler(IArticleRepository articleRepository, IMapper mapper)
    : IQueryHandler<GetFeaturedArticlesQuery, GetFeaturedArticlesResult>
{
    /// <inheritdoc />
    public async Task<GetFeaturedArticlesResult> Handle(
        GetFeaturedArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ArticleEntity> articles = await articleRepository.GetFeaturedAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleSummaryDto> dtoList = articles.ToArticleSummaryDtos(mapper);

        return new GetFeaturedArticlesResult(Articles: dtoList);
    }
}
