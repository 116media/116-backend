using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnBookmarkedShortVideos;

/// <summary>
/// Query for the authenticated user's bookmarked short videos.
/// </summary>
/// <param name="UserId">The requesting user's identity UUID.</param>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record PublicGetOwnBookmarkedShortVideosQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnBookmarkedShortVideosResult>;

/// <summary>
/// Contains the authenticated user's paginated bookmarked short videos.
/// </summary>
/// <param name="ShortVideos">The paginated favorite activity rows.</param>
public record PublicGetOwnBookmarkedShortVideosResult(PaginatedResult<UserShortVideoActivityDto> ShortVideos);
