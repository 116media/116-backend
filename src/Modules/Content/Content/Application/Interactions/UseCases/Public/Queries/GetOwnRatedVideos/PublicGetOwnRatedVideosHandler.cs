using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnRatedVideos;

/// <summary>
/// Handles the current-user rated-video collection query.
/// </summary>
public class PublicGetOwnRatedVideosHandler(
    IVideoRepository videoRepository,
    IFileStorageService fileStorage,
    IContentLookupFactory contentLookupFactory
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
        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            thumbnailIds,
            cancellationToken
        );

        IReadOnlySet<Guid> videosWithLyrics = await videoRepository.GetIdsWithPublishedLyricsAsync(
            videoIds: activities.Select(activity => activity.Video.Id).ToList(),
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicVideoSummaryDto> videoDtos = activities
            .Select(activity => activity.Video)
            .ToList()
            .ToPublicVideoSummaryDtos(
                await contentLookupFactory.ResolveForVideosAsync(
                    [.. activities.Select(activity => activity.Video)],
                    cancellationToken
                ),
                files,
                videosWithLyrics
            );

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
