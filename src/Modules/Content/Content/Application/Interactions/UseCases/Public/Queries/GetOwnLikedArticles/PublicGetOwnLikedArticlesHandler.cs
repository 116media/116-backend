using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnLikedArticles;

/// <summary>
/// Handles the current-user liked article query.
/// </summary>
public class PublicGetOwnLikedArticlesHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetOwnLikedArticlesQuery, PublicGetOwnLikedArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnLikedArticlesResult> Handle(
        PublicGetOwnLikedArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;
        (List<ArticleActivity> activities, int totalCount) = await articleRepository.GetLikedArticlesAsync(
            query.UserId,
            pageIndex + 1,
            pageSize,
            cancellationToken
        );

        Guid[] articleIds = activities.Select(activity => activity.Article.Id).ToArray();
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await articleRepository.GetLikedAndBookmarkedIdsAsync(query.UserId, articleIds, cancellationToken);

        var items = new List<UserArticleActivityDto>(activities.Count);
        foreach (ArticleActivity activity in activities)
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
            items.Add(new UserArticleActivityDto(article, activity.LastInteractedAt, 1));
        }

        return new PublicGetOwnLikedArticlesResult(
            new PaginatedResult<UserArticleActivityDto>(pageIndex, pageSize, totalCount, items)
        );
    }
}
