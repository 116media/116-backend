using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.Contracts;

/// <summary>
/// Pages the published videos of the exclusive category into public summaries.
/// </summary>
public interface IPublicExclusiveCategoryVideosService
{
    /// <summary>
    /// Loads one page of the category's published videos and assembles their public summaries.
    /// </summary>
    /// <param name="categoryId">The exclusive category.</param>
    /// <param name="pageIndex">The zero-based page.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<PaginatedResult<PublicVideoSummaryDto>> GetPublishedPageAsync(
        Guid categoryId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken
    );
}
