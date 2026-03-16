using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Handles the <see cref="AdminUploadVideoThumbnailCommand" /> to upload or replace a video thumbnail.
/// Deletes the old Cloudinary asset after the new thumbnail is uploaded successfully.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cloudinaryService">Service for uploading and deleting Cloudinary image assets.</param>
public class AdminUploadVideoThumbnailHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    ICloudinaryService cloudinaryService
) : ICommandHandler<AdminUploadVideoThumbnailCommand, AdminUploadVideoThumbnailResult>
{
    /// <inheritdoc />
    public async Task<AdminUploadVideoThumbnailResult> Handle(
        AdminUploadVideoThumbnailCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid videoId = Guid.Parse(command.VideoId);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: videoId,
            cancellationToken: cancellationToken
        );

        string? oldKey = video.ThumbnailStorageKey;

        string storageKey = videoId.ToString();

        CloudinaryUploadResult result = await cloudinaryService.UploadImageAsync(
            file: command.File,
            publicId: storageKey,
            folder: "content/video-thumbnails",
            cancellationToken: cancellationToken
        );

        video.UpdateThumbnail(thumbnailUrl: result.SecureUrl, thumbnailStorageKey: result.PublicId);

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        if (oldKey is not null)
        {
            await cloudinaryService.DeleteImageAsync(storageKey: oldKey, cancellationToken: cancellationToken);
        }

        return new AdminUploadVideoThumbnailResult(
            ThumbnailUrl: result.SecureUrl,
            ThumbnailStorageKey: result.PublicId
        );
    }
}
