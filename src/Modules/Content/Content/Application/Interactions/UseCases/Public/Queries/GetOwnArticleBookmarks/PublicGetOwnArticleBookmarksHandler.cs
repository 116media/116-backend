using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnArticleBookmarks;

/// <summary>
/// Handles the <see cref="PublicGetOwnArticleBookmarksQuery" /> to retrieve the user's bookmarked articles.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="fileRepository">Repository for resolving file URLs.</param>
/// <param name="mapper">The Mapster mapper instance.</param>
public class PublicGetOwnArticleBookmarksHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetOwnArticleBookmarksQuery, PublicGetOwnArticleBookmarksResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnArticleBookmarksResult> Handle(
        PublicGetOwnArticleBookmarksQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;

        (List<BookmarkedArticleActivity> activities, int totalCount) =
            await articleRepository.GetBookmarkedArticlesAsync(
                userId: query.UserId,
                page: pageIndex + 1,
                pageSize: pageSize,
                cancellationToken: cancellationToken
            );

        Guid[] articleIds = activities.Select(activity => activity.Article.Id).ToArray();
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await articleRepository.GetLikedAndBookmarkedIdsAsync(query.UserId, articleIds, cancellationToken);

        var dtoList = new List<UserBookmarkedArticleDto>(activities.Count);
        foreach (BookmarkedArticleActivity activity in activities)
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
            dtoList.Add(new UserBookmarkedArticleDto(article, activity.BookmarkedAt));
        }

        var paginated = new PaginatedResult<UserBookmarkedArticleDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetOwnArticleBookmarksResult(Articles: paginated);
    }
}
