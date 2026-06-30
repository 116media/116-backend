using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;

/// <summary>
/// Handles the <see cref="PublicGetArticleBySlugQuery" /> to retrieve a single published article by its slug.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="fileRepository">Repository for resolving file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetArticleBySlugHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : IQueryHandler<PublicGetArticleBySlugQuery, PublicGetArticleBySlugResult>
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
            throw i18n.Article.NotFound(Guid.Empty);
        }

        bool isLiked = false;
        bool isBookmarked = false;

        if (query.CurrentUserId is Guid userId)
        {
            isLiked = await articleRepository.HasLikedAsync(
                userId: userId,
                articleId: article.Id,
                cancellationToken: cancellationToken
            );
            isBookmarked = await articleRepository.HasBookmarkedAsync(
                userId: userId,
                articleId: article.Id,
                cancellationToken: cancellationToken
            );
        }

        var dto = await article.ToArticleDetailDtoAsync(
            mapper,
            fileRepository,
            cancellationToken,
            isLiked: isLiked,
            isBookmarked: isBookmarked
        );
        return new PublicGetArticleBySlugResult(Article: dto);
    }
}
