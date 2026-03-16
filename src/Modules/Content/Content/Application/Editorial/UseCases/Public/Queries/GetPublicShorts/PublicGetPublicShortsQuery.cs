using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;

/// <summary>
/// Query for retrieving a paginated list of active short videos for public consumption.
/// Supports optional search by title.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Search">Optional search term matched against title.</param>
public record PublicGetPublicShortsQuery(PaginatedRequest PaginatedRequest, string? Search)
    : IQuery<PublicGetPublicShortsResult>;

/// <summary>
/// Result of the <see cref="PublicGetPublicShortsQuery" /> containing a paginated list of short video DTOs.
/// </summary>
/// <param name="ShortVideos">The paginated result containing short video DTOs.</param>
public record PublicGetPublicShortsResult(PaginatedResult<ShortVideoDto> ShortVideos);
