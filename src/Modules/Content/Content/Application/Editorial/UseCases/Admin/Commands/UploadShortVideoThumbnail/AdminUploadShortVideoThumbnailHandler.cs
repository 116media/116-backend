using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Handles the <see cref="AdminUploadShortVideoThumbnailCommand" /> to upload or replace a short video thumbnail.
/// The thumbnail file is tracked via <see cref="FileEntity" /> in the Core module.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadShortVideoThumbnailHandler(
    IShortVideoRepository shortVideoRepository,
    IFileRepository fileRepository,
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

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: shortVideo.ThumbnailFileId,
            file: command.File,
            publicId: shortVideoId.ToString(),
            folder: "content/short-video-thumbnails",
            originalFileName: command.File.FileName,
            mimeType: command.File.ContentType,
            cancellationToken: cancellationToken
        );

        shortVideo.SetThumbnailFileId(thumbnailFileId: fileEntity.Id);

        shortVideoRepository.Update(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUploadShortVideoThumbnailResult(
            ThumbnailUrl: fileEntity.StorageUrl,
            ThumbnailStorageKey: fileEntity.StorageKey!
        );
    }
}
