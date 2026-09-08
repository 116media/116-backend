using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Handles the <see cref="AdminUploadVideoThumbnailCommand" /> to upload or replace a video thumbnail.
/// The thumbnail file is tracked via <see cref="FileReferenceDto" /> in the Core module.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadVideoThumbnailHandler(
    IVideoRepository videoRepository,
    IFileStorageService fileStorage,
    IContentUnitOfWork unitOfWork
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

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: command.File,
            publicId: videoId.ToString(),
            folder: "content/video-thumbnails",
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileStorage.RecordAsync(
                    file: uploaded,
                    supersededFileId: video.ThumbnailFileId,
                    cancellationToken: ct
                );

                video.SetThumbnailFileId(thumbnailFileId: uploaded.Reference.Id);

                videoRepository.Update(video: video);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadVideoThumbnailResult(
            ThumbnailUrl: uploaded.Reference.StorageUrl,
            ThumbnailStorageKey: uploaded.Reference.StorageKey!
        );
    }
}
