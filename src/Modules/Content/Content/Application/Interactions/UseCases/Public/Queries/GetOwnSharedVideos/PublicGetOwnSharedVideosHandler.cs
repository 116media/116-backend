using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedVideos;

/// <summary>
/// Handles the current-user shared-video collection query.
/// </summary>
public class PublicGetOwnSharedVideosHandler(
    IVideoRepository videoRepository,
    IFileStorageService fileStorage,
    IContentLookupFactory contentLookupFactory
) : IQueryHandler<PublicGetOwnSharedVideosQuery, PublicGetOwnSharedVideosResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnSharedVideosResult> Handle(
        PublicGetOwnSharedVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;
        (IReadOnlyList<SharedVideoActivity> activities, int totalCount) =
            await videoRepository.GetSharedVideosByUserAsync(query.UserId, pageIndex + 1, pageSize, cancellationToken);

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
                        InteractionCount: activity.ShareCount,
                        LastShareChannel: activity.LastShareChannel
                    )
            )
            .ToList();

        return new PublicGetOwnSharedVideosResult(
            new PaginatedResult<UserVideoActivityDto>(pageIndex, pageSize, totalCount, items)
        );
    }
}
