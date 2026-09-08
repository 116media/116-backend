using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;

/// <summary>
/// Handles the <see cref="PublicGetPublishedArticlesQuery" /> to retrieve a paginated list of published articles.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="articleInteractionRepository">Repository for article interaction data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class PublicGetPublishedArticlesHandler(
    IArticleRepository articleRepository,
    IArticleInteractionRepository articleInteractionRepository,
    IFileStorageService fileStorage
) : IQueryHandler<PublicGetPublishedArticlesQuery, PublicGetPublishedArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPublishedArticlesResult> Handle(
        PublicGetPublishedArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<ArticleEntity> articles, int totalCount) = await articleRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            status: EnumContentStatus.Published,
            categoryId: query.CategoryId,
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

        var paginatedResult = new PaginatedResult<PublicArticleSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetPublishedArticlesResult(Articles: paginatedResult);
    }
}
