using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShortsAdmin;

/// <summary>
/// Query for retrieving a paginated list of short videos for admin management.
/// Supports optional filtering by active status.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="IsActive">Optional filter by active status.</param>
public record GetAllShortsAdminQuery(PaginatedRequest PaginatedRequest, bool? IsActive)
    : IQuery<GetAllShortsAdminResult>;

/// <summary>
/// Result of the <see cref="GetAllShortsAdminQuery" /> containing a paginated list of short video DTOs.
/// </summary>
/// <param name="ShortVideos">The paginated result containing short video DTOs.</param>
public record GetAllShortsAdminResult(PaginatedResult<ShortVideoDto> ShortVideos);
