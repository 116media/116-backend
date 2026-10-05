using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Storage.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArticles;

/// <summary>
/// Handles the <see cref="AdminGetAllArticlesQuery" /> to retrieve a paginated list of articles.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="fileStorage">Storage's file contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllArticlesHandler(
    IArticleRepository articleRepository,
    IFileStorageService fileStorage,
    IMapper mapper,
    IContentLookupService contentLookupService
) : IQueryHandler<AdminGetAllArticlesQuery, AdminGetAllArticlesResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllArticlesResult> Handle(
        AdminGetAllArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<ArticleEntity> articles, int totalCount) = await articleRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            status: query.Status,
            categoryId: query.CategoryId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleSummaryDto> dtoList = await articles.ToArticleSummaryDtosAsync(
            mapper,
            await contentLookupService.ResolveForArticlesAsync(articles, cancellationToken),
            fileStorage,
            cancellationToken
        );

        var paginatedResult = new PaginatedResult<ArticleSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllArticlesResult(Articles: paginatedResult);
    }
}
