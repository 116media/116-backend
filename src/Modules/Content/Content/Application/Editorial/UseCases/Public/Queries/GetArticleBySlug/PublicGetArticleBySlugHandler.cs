using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;

/// <summary>
/// Handles the <see cref="PublicGetArticleBySlugQuery" /> to retrieve a single published article by its slug.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetArticleBySlugHandler(IArticleRepository articleRepository, IMapper mapper)
    : IQueryHandler<PublicGetArticleBySlugQuery, PublicGetArticleBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetArticleBySlugResult> Handle(
        PublicGetArticleBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity? article = await articleRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (article is null || article.Status != EnumContentStatus.Published)
        {
            throw ArticleErrors.NotFound(Guid.Empty);
        }

        var dto = article.ToArticleDetailDto(mapper);
        return new PublicGetArticleBySlugResult(Article: dto);
    }
}
