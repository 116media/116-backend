using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;

/// <summary>
/// Handles the <see cref="PublicGetArticleBySlugQuery" /> to serve a published article.
/// </summary>
/// <param name="articleRepository">Repository resolving the article.</param>
/// <param name="articleInteractionRepository">Repository resolving the reader's like and bookmark state.</param>
/// <param name="articleDtoService">Service assembling the public article detail.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetArticleBySlugHandler(
    IArticleRepository articleRepository,
    IArticleInteractionRepository articleInteractionRepository,
    IArticleDtoService articleDtoService,
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
            isLiked = await articleInteractionRepository.HasLikedAsync(
                userId: userId,
                articleId: article.Id,
                cancellationToken: cancellationToken
            );
            isBookmarked = await articleInteractionRepository.HasBookmarkedAsync(
                userId: userId,
                articleId: article.Id,
                cancellationToken: cancellationToken
            );
        }

        var dto = await articleDtoService.CreatePublicDetailAsync(article, isLiked, isBookmarked, cancellationToken);
        return new PublicGetArticleBySlugResult(Article: dto);
    }
}
