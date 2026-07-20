using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnLikedShortVideos;

/// <summary>
/// Query for the authenticated user's liked short videos.
/// </summary>
/// <param name="UserId">The requesting user's identity UUID.</param>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record PublicGetOwnLikedShortVideosQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnLikedShortVideosResult>;

/// <summary>
/// Contains the authenticated user's paginated liked short videos.
/// </summary>
/// <param name="ShortVideos">The paginated favorite activity rows.</param>
public record PublicGetOwnLikedShortVideosResult(PaginatedResult<UserShortVideoActivityDto> ShortVideos);
