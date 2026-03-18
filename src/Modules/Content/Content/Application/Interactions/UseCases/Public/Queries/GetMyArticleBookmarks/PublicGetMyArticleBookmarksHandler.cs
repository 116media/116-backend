using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks;

/// <summary>
/// Handles the <see cref="PublicGetMyArticleBookmarksQuery" /> to retrieve the user's bookmarked articles.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="mapper">The Mapster mapper instance.</param>
public class PublicGetMyArticleBookmarksHandler(IArticleRepository articleRepository, IMapper mapper)
    : IQueryHandler<PublicGetMyArticleBookmarksQuery, PublicGetMyArticleBookmarksResult>
{
    /// <inheritdoc />
    public async Task<PublicGetMyArticleBookmarksResult> Handle(
        PublicGetMyArticleBookmarksQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;

        (List<ArticleEntity> articles, int totalCount) = await articleRepository.GetBookmarkedArticlesAsync(
            userId: query.UserId,
            page: pageIndex + 1,
            pageSize: pageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleSummaryDto> dtoList = articles.ToArticleSummaryDtos(mapper);

        var paginated = new PaginatedResult<ArticleSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetMyArticleBookmarksResult(Articles: paginated);
    }
}
