using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Handles the <see cref="AdminUploadVideoThumbnailCommand" /> to upload or replace a video thumbnail.
/// The thumbnail file is tracked via <see cref="FileEntity" /> in the Core module.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileUploadService">Uploads and replaces stored assets.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadVideoThumbnailHandler(
    IVideoRepository videoRepository,
    IFileUploadService fileUploadService,
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

        FileEntity uploaded = await fileUploadService.UploadImageAsync(
            file: command.File,
            publicId: videoId.ToString(),
            folder: "content/video-thumbnails",
            originalFileName: command.File.FileName,
            mimeType: command.File.ContentType,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileUploadService.RecordAsync(
                    file: uploaded,
                    supersededFileId: video.ThumbnailFileId,
                    cancellationToken: ct
                );

                video.SetThumbnailFileId(thumbnailFileId: uploaded.Id);

                videoRepository.Update(video: video);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadVideoThumbnailResult(
            ThumbnailUrl: uploaded.StorageUrl,
            ThumbnailStorageKey: uploaded.StorageKey!
        );
    }
}
