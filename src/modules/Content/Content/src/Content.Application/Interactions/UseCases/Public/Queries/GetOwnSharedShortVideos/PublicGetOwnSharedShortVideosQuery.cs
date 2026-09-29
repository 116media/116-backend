using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedShortVideos;

/// <summary>
/// Query for short videos shared by the authenticated user.
/// </summary>
/// <param name="UserId">The requesting user's identity UUID.</param>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record PublicGetOwnSharedShortVideosQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnSharedShortVideosResult>;

/// <summary>
/// Contains the authenticated user's paginated shared short videos.
/// </summary>
/// <param name="ShortVideos">The grouped and paginated favorite activity rows.</param>
public record PublicGetOwnSharedShortVideosResult(PaginatedResult<UserShortVideoActivityDto> ShortVideos);
