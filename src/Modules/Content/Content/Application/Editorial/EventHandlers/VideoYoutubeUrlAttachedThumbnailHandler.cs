using System.Text.RegularExpressions;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Editorial.EventHandlers;

/// <summary>
/// Downloads the YouTube thumbnail for a freshly attached video URL and
/// attaches it to the video post-commit, in its own scope and commit. The
/// attach command no longer fails on thumbnail outages: a download or upload
/// failure is logged and tolerated by the publisher, and the video renders
/// thumbnail-less until a later attach lands the asset.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work committing the thumbnail attachment.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="youtubeThumbnailService">Service for downloading YouTube video thumbnails.</param>
/// <param name="logger">Logger for skipped thumbnail resolutions.</param>
public class VideoYoutubeUrlAttachedThumbnailHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IYoutubeThumbnailService youtubeThumbnailService,
    ILogger<VideoYoutubeUrlAttachedThumbnailHandler> logger
) : IDomainEventHandler<VideoYoutubeUrlAttachedEvent>
{
    private static readonly Regex YoutubeIdRegex = new(
        @"(?:youtube\.com/(?:watch\?v=|embed/|shorts/)|youtu\.be/)([A-Za-z0-9_-]{11})",
        RegexOptions.Compiled
    );

    /// <inheritdoc />
    public async Task Handle(VideoYoutubeUrlAttachedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Match match = YoutubeIdRegex.Match(domainEvent.YoutubeVideoUrl);
        if (!match.Success)
        {
            logger.LogWarning(
                "Skipping thumbnail resolution for video {VideoId}: no YouTube video ID in {YoutubeVideoUrl}.",
                domainEvent.VideoId,
                domainEvent.YoutubeVideoUrl
            );
            return;
        }

        string youtubeVideoId = match.Groups[1].Value;

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: domainEvent.VideoId,
            cancellationToken: cancellationToken
        );

        IFormFile thumbnail = await youtubeThumbnailService.DownloadThumbnailAsync(
            youtubeVideoId: youtubeVideoId,
            cancellationToken: cancellationToken
        );

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: video.ThumbnailFileId,
            file: thumbnail,
            publicId: video.Id.ToString(),
            folder: "content/video-thumbnails",
            originalFileName: $"{youtubeVideoId}-thumbnail.jpg",
            mimeType: "image/jpeg",
            cancellationToken: cancellationToken
        );

        video.SetThumbnailFileId(thumbnailFileId: fileEntity.Id);

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
