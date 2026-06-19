using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;

/// <summary>
/// Query for retrieving the exclusive category and its published videos.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters for the video list.</param>
public record PublicGetExclusiveCategoryQuery(PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetExclusiveCategoryResult>;

/// <summary>
/// Result of the <see cref="PublicGetExclusiveCategoryQuery" /> containing the exclusive category and its videos.
/// </summary>
/// <param name="Category">The exclusive category DTO.</param>
/// <param name="Videos">Paginated list of published videos in the exclusive category.</param>
public record PublicGetExclusiveCategoryResult(CategoryDto Category, PaginatedResult<VideoSummaryDto> Videos);
