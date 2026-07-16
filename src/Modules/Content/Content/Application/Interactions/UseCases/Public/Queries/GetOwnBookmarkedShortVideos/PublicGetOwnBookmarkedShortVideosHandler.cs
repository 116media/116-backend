using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnBookmarkedShortVideos;

/// <summary>
/// Handles requests for the authenticated user's bookmarked short videos.
/// </summary>
public class PublicGetOwnBookmarkedShortVideosHandler(
    IShortVideoRepository shortVideoRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetOwnBookmarkedShortVideosQuery, PublicGetOwnBookmarkedShortVideosResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnBookmarkedShortVideosResult> Handle(
        PublicGetOwnBookmarkedShortVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;
        (List<ShortVideoActivity> items, int totalCount) = await shortVideoRepository.GetBookmarkedShortVideosAsync(
            query.UserId,
            pageIndex + 1,
            pageSize,
            cancellationToken
        );
        List<ShortVideoEntity> shortVideos = items.Select(item => item.ShortVideo).ToList();
        HashSet<Guid> ids = shortVideos.Select(shortVideo => shortVideo.Id).ToHashSet();
        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await shortVideoRepository.GetLikedAndBookmarkedIdsAsync(query.UserId, ids, cancellationToken);
        IReadOnlyList<ShortVideoDto> dtos = await shortVideos.ToShortVideoDtosAsync(
            mapper,
            fileRepository,
            liked,
            bookmarked,
            cancellationToken
        );
        List<UserShortVideoActivityDto> activity = items
            .Select(
                (item, index) =>
                    new UserShortVideoActivityDto(dtos[index], item.LastInteractedAt, item.InteractionCount)
            )
            .ToList();

        return new PublicGetOwnBookmarkedShortVideosResult(
            new PaginatedResult<UserShortVideoActivityDto>(pageIndex, pageSize, totalCount, activity)
        );
    }
}
