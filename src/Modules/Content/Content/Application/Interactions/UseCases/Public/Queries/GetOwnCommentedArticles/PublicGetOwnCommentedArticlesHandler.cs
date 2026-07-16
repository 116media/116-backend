using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentedArticles;

/// <summary>
/// Handles the current-user grouped article comment query.
/// </summary>
public class PublicGetOwnCommentedArticlesHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMapper mapper
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
        (List<CommentedArticleActivity> activities, int totalCount) = await articleRepository.GetCommentedArticlesAsync(
            query.UserId,
            pageIndex + 1,
            pageSize,
            cancellationToken
        );

        Guid[] articleIds = activities.Select(activity => activity.Article.Id).ToArray();
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await articleRepository.GetLikedAndBookmarkedIdsAsync(query.UserId, articleIds, cancellationToken);

        var items = new List<UserCommentedArticleDto>(activities.Count);
        foreach (CommentedArticleActivity activity in activities)
        {
            ArticleSummaryDto article = await activity.Article.ToArticleSummaryDtoAsync(
                mapper,
                fileRepository,
                cancellationToken
            );
            article = article with
            {
                IsLiked = liked.Contains(activity.Article.Id),
                IsBookmarked = bookmarked.Contains(activity.Article.Id),
            };
            ArticleCommentDto comment = activity.LatestComment.ToArticleCommentDto(mapper);
            items.Add(new UserCommentedArticleDto(article, comment, activity.CommentCount, activity.LastCommentedAt));
        }

        return new PublicGetOwnCommentedArticlesResult(
            new PaginatedResult<UserCommentedArticleDto>(pageIndex, pageSize, totalCount, items)
        );
    }
}
