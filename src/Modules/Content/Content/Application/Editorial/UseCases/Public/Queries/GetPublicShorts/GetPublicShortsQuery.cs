using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;

/// <summary>
/// Query for retrieving a paginated list of active short videos for public consumption.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
public record GetPublicShortsQuery(PaginatedRequest PaginatedRequest) : IQuery<GetPublicShortsResult>;

/// <summary>
/// Result of the <see cref="GetPublicShortsQuery" /> containing a paginated list of short video DTOs.
/// </summary>
/// <param name="ShortVideos">The paginated result containing short video DTOs.</param>
public record GetPublicShortsResult(PaginatedResult<ShortVideoDto> ShortVideos);
