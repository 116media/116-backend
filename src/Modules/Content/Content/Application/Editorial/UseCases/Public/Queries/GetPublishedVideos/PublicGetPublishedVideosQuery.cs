using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;

/// <summary>
/// Query for retrieving a paginated list of published videos for public consumption.
/// Supports optional filtering by category and search term.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Search">Optional search term matched against title, description, meta title, and meta description.</param>
/// <param name="CategoryId">Optional filter by category identifier.</param>
public record PublicGetPublishedVideosQuery(PaginatedRequest PaginatedRequest, string? Search, Guid? CategoryId)
    : IQuery<PublicGetPublishedVideosResult>;

/// <summary>
/// Result of the <see cref="PublicGetPublishedVideosQuery" /> containing a paginated list of video summaries.
/// </summary>
/// <param name="Videos">The paginated result containing video summary DTOs.</param>
public record PublicGetPublishedVideosResult(PaginatedResult<VideoSummaryDto> Videos);
