using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideosAdmin;

/// <summary>
/// Query for retrieving a paginated list of videos for admin management.
/// Supports optional filtering by status and category.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Status">Optional filter by content status.</param>
/// <param name="CategoryId">Optional filter by category identifier.</param>
public record GetAllVideosAdminQuery(PaginatedRequest PaginatedRequest, EnumContentStatus? Status, Guid? CategoryId)
    : IQuery<GetAllVideosAdminResult>;

/// <summary>
/// Result of the <see cref="GetAllVideosAdminQuery" /> containing a paginated list of video summaries.
/// </summary>
/// <param name="Videos">The paginated result containing video summary DTOs.</param>
public record GetAllVideosAdminResult(PaginatedResult<VideoSummaryDto> Videos);
