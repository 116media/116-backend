using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Handles the <see cref="AdminUploadShortVideoThumbnailCommand" /> to upload or replace a short video thumbnail.
/// The thumbnail file is tracked via <see cref="FileEntity" /> in the Core module.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="fileUploadService">Uploads and replaces stored assets.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadShortVideoThumbnailHandler(
    IShortVideoRepository shortVideoRepository,
    IFileUploadService fileUploadService,
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

        FileEntity uploaded = await fileUploadService.UploadImageAsync(
            file: command.File,
            publicId: shortVideoId.ToString(),
            folder: "content/short-video-thumbnails",
            originalFileName: command.File.FileName,
            mimeType: command.File.ContentType,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileUploadService.RecordAsync(
                    file: uploaded,
                    supersededFileId: shortVideo.ThumbnailFileId,
                    cancellationToken: ct
                );

                shortVideo.SetThumbnailFileId(thumbnailFileId: uploaded.Id);

                shortVideoRepository.Update(shortVideo: shortVideo);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadShortVideoThumbnailResult(
            ThumbnailUrl: uploaded.StorageUrl,
            ThumbnailStorageKey: uploaded.StorageKey!
        );
    }
}
