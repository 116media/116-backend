using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;

/// <summary>
/// Query for retrieving a paginated list of published videos for public consumption.
/// Supports optional filtering by category.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="CategoryId">Optional filter by category identifier.</param>
public record GetPublishedVideosQuery(PaginatedRequest PaginatedRequest, Guid? CategoryId)
    : IQuery<GetPublishedVideosResult>;

/// <summary>
/// Result of the <see cref="GetPublishedVideosQuery" /> containing a paginated list of video summaries.
/// </summary>
/// <param name="Videos">The paginated result containing video summary DTOs.</param>
public record GetPublishedVideosResult(PaginatedResult<VideoSummaryDto> Videos);
