using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentedArticles;

/// <summary>
/// Handles the current-user grouped article comment query.
/// </summary>
public class PublicGetOwnCommentedArticlesHandler(
    IArticleCommentRepository articleCommentRepository,
    IArticleInteractionRepository articleInteractionRepository,
    IFileStorageService fileStorage,
    IContentLookupFactory contentLookupFactory
) : IQueryHandler<PublicGetOwnCommentedArticlesQuery, PublicGetOwnCommentedArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnCommentedArticlesResult> Handle(
        PublicGetOwnCommentedArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;
        (List<CommentedArticleActivity> activities, int totalCount) =
            await articleCommentRepository.GetCommentedArticlesAsync(
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

        var items = new List<UserCommentedArticleDto>(activities.Count);
        foreach (CommentedArticleActivity activity in activities)
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
            PublicArticleCommentDto comment = activity.LatestComment.ToPublicArticleCommentDto();
            items.Add(new UserCommentedArticleDto(article, comment, activity.CommentCount, activity.LastCommentedAt));
        }

        return new PublicGetOwnCommentedArticlesResult(
            new PaginatedResult<UserCommentedArticleDto>(pageIndex, pageSize, totalCount, items)
        );
    }
}
