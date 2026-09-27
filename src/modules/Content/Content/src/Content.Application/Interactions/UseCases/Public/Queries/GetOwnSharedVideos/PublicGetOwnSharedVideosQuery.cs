using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedVideos;

/// <summary>
/// Retrieves published videos shared by the authenticated user.
/// </summary>
public record PublicGetOwnSharedVideosQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnSharedVideosResult>;

/// <summary>
/// Paginated shared-video result.
/// </summary>
public record PublicGetOwnSharedVideosResult(PaginatedResult<UserVideoActivityDto> Videos);
