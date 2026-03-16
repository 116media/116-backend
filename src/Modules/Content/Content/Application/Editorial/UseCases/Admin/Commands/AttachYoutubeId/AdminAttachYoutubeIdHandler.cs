using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;

/// <summary>
/// Handles the <see cref="AdminAttachYoutubeIdCommand" /> to attach a YouTube video ID and
/// automatically download and re-upload the YouTube thumbnail to Cloudinary.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cloudinaryService">Service for uploading and deleting Cloudinary image assets.</param>
/// <param name="youtubeThumbnailService">Service for downloading YouTube video thumbnails.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAttachYoutubeIdHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    ICloudinaryService cloudinaryService,
    IYoutubeThumbnailService youtubeThumbnailService,
    IMapper mapper
) : ICommandHandler<AdminAttachYoutubeIdCommand, AdminAttachYoutubeIdResult>
{
    /// <inheritdoc />
    public async Task<AdminAttachYoutubeIdResult> Handle(
        AdminAttachYoutubeIdCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid videoId = Guid.Parse(command.VideoId);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: videoId,
            cancellationToken: cancellationToken
        );

        string? oldThumbnailStorageKey = video.ThumbnailStorageKey;

        video.AttachYoutubeId(youtubeVideoId: command.YoutubeVideoId);

        IFormFile thumbnail = await youtubeThumbnailService.DownloadThumbnailAsync(
            youtubeVideoId: command.YoutubeVideoId,
            cancellationToken: cancellationToken
        );

        string storageKey = videoId.ToString();

        CloudinaryUploadResult uploadResult = await cloudinaryService.UploadImageAsync(
            file: thumbnail,
            publicId: storageKey,
            folder: "content/video-thumbnails",
            cancellationToken: cancellationToken
        );

        video.UpdateThumbnail(thumbnailUrl: uploadResult.SecureUrl, thumbnailStorageKey: uploadResult.PublicId);

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        if (oldThumbnailStorageKey is not null)
        {
            await cloudinaryService.DeleteImageAsync(
                storageKey: oldThumbnailStorageKey,
                cancellationToken: cancellationToken
            );
        }

        VideoEntity updated = await videoRepository.GetByIdOrThrowAsync(
            id: videoId,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToVideoDetailDto(mapper);
        return new AdminAttachYoutubeIdResult(Video: dto);
    }
}
