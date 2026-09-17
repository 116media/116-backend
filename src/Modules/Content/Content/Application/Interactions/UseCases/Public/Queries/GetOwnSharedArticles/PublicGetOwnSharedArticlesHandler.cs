using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedArticles;

/// <summary>
/// Handles the current-user grouped article share query.
/// </summary>
public class PublicGetOwnSharedArticlesHandler(
    IArticleInteractionRepository articleInteractionRepository,
    IFileStorageService fileStorage,
    IContentLookupFactory contentLookupFactory
) : IQueryHandler<PublicGetOwnSharedArticlesQuery, PublicGetOwnSharedArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnSharedArticlesResult> Handle(
        PublicGetOwnSharedArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;
        (List<ArticleActivity> activities, int totalCount) = await articleInteractionRepository.GetSharedArticlesAsync(
            query.UserId,
            pageIndex + 1,
            pageSize,
            cancellationToken
        );

        Guid[] articleIds = activities.Select(activity => activity.Article.Id).ToArray();
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await articleInteractionRepository.GetLikedAndBookmarkedIdsAsync(
                query.UserId,
                articleIds,
                cancellationToken
            );

        var items = new List<UserArticleActivityDto>(activities.Count);
        foreach (ArticleActivity activity in activities)
        {
            PublicArticleSummaryDto article = await activity.Article.ToPublicArticleSummaryDtoAsync(
                await contentLookupFactory.ResolveForArticlesAsync([activity.Article], cancellationToken),
                fileStorage,
                cancellationToken
            );
            article = article with
            {
                IsLiked = liked.Contains(activity.Article.Id),
                IsBookmarked = bookmarked.Contains(activity.Article.Id),
            };
            items.Add(
                new UserArticleActivityDto(
                    article,
                    activity.LastInteractedAt,
                    activity.InteractionCount,
                    activity.LastShareChannel
                )
            );
        }

        return new PublicGetOwnSharedArticlesResult(
            new PaginatedResult<UserArticleActivityDto>(pageIndex, pageSize, totalCount, items)
        );
    }
}
