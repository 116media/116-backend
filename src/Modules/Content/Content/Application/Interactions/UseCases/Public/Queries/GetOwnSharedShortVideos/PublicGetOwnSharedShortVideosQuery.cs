using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

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
