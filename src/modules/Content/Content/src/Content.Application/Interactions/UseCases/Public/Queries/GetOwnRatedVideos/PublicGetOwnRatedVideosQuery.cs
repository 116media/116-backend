using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnRatedVideos;

/// <summary>
/// Retrieves the authenticated user's current ratings of published videos.
/// </summary>
public record PublicGetOwnRatedVideosQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnRatedVideosResult>;

/// <summary>
/// Paginated rated-video result.
/// </summary>
public record PublicGetOwnRatedVideosResult(PaginatedResult<UserVideoActivityDto> Videos);
