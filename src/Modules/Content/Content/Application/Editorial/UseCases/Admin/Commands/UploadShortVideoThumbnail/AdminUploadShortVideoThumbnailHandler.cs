using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Handles the <see cref="AdminUploadShortVideoThumbnailCommand" /> to upload or replace a short video thumbnail.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="cloudinaryService">Service for uploading and deleting media assets in cloud storage.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadShortVideoThumbnailHandler(
    IShortVideoRepository shortVideoRepository,
    ICloudinaryService cloudinaryService,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminUploadShortVideoThumbnailCommand, AdminUploadShortVideoThumbnailResult>
{
    /// <inheritdoc />
    public async Task<AdminUploadShortVideoThumbnailResult> Handle(
        AdminUploadShortVideoThumbnailCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid shortVideoId = Guid.Parse(command.ShortVideoId);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: shortVideoId,
            cancellationToken: cancellationToken
        );

        string? oldKey = shortVideo.ThumbnailStorageKey;

        string storageKey = shortVideoId.ToString();

        CloudinaryUploadResult result = await cloudinaryService.UploadImageAsync(
            file: command.File,
            publicId: storageKey,
            folder: "content/short-video-thumbnails",
            cancellationToken: cancellationToken
        );

        shortVideo.UpdateThumbnail(thumbnailUrl: result.SecureUrl, thumbnailStorageKey: result.PublicId);

        shortVideoRepository.Update(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        if (oldKey is not null)
        {
            await cloudinaryService.DeleteImageAsync(storageKey: oldKey, cancellationToken: cancellationToken);
        }

        return new AdminUploadShortVideoThumbnailResult(
            ThumbnailUrl: result.SecureUrl,
            ThumbnailStorageKey: result.PublicId
        );
    }
}
