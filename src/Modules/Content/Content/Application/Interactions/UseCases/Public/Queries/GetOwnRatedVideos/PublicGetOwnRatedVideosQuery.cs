using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

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
