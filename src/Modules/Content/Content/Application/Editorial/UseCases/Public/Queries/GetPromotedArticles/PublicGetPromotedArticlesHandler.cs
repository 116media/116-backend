using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;

/// <summary>
/// Handles the <see cref="PublicGetPromotedArticlesQuery" /> to retrieve all currently promoted published articles.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="articleInteractionRepository">Repository for article interaction data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class PublicGetPromotedArticlesHandler(
    IArticleRepository articleRepository,
    IArticleInteractionRepository articleInteractionRepository,
    IFileStorageService fileStorage
) : IQueryHandler<PublicGetPromotedArticlesQuery, PublicGetPromotedArticlesResult>
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

        List<Guid> articleIds = articles.Select(article => article.Id).ToList();
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await articleInteractionRepository.GetLikedAndBookmarkedIdsAsync(
                currentUserId: query.CurrentUserId,
                articleIds: articleIds,
                cancellationToken: cancellationToken
            );

        IReadOnlyList<PublicArticleSummaryDto> dtoList = await articles.ToPublicArticleSummaryDtosAsync(
            fileStorage,
            liked,
            bookmarked,
            cancellationToken
        );

        return new PublicGetPromotedArticlesResult(Articles: dtoList);
    }
}
