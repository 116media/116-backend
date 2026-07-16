using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnRatedVideos;

/// <summary>
/// Handles the current-user rated-video collection query.
/// </summary>
public class PublicGetOwnRatedVideosHandler(
    IVideoRepository videoRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetOwnRatedVideosQuery, PublicGetOwnRatedVideosResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnRatedVideosResult> Handle(
        PublicGetOwnRatedVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;
        (IReadOnlyList<RatedVideoActivity> activities, int totalCount) =
            await videoRepository.GetRatedVideosByUserAsync(query.UserId, pageIndex + 1, pageSize, cancellationToken);

        Guid[] thumbnailIds = activities
            .Select(activity => activity.Video.ThumbnailFileId)
            .OfType<Guid>()
            .Distinct()
            .ToArray();
        IReadOnlyDictionary<Guid, FileEntity> files = await fileRepository.GetByIdsAsync(
            thumbnailIds,
            cancellationToken
        );

        IReadOnlyList<VideoSummaryDto> videoDtos = activities
            .Select(activity => activity.Video)
            .ToList()
            .ToVideoSummaryDtos(mapper, files);

        IReadOnlyList<UserVideoActivityDto> items = activities
            .Select(
                (activity, index) =>
                    new UserVideoActivityDto(
                        Video: videoDtos[index],
                        LastInteractedAt: activity.LastInteractedAt,
                        InteractionCount: 1,
                        RatedStars: activity.Stars
                    )
            )
            .ToList();

        return new PublicGetOwnRatedVideosResult(
            new PaginatedResult<UserVideoActivityDto>(pageIndex, pageSize, totalCount, items)
        );
    }
}
