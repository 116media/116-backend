using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Handles the <see cref="AdminUploadVideoThumbnailCommand" /> to upload or replace a video thumbnail.
/// The thumbnail file is tracked via <see cref="FileEntity" /> in the Core module.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadVideoThumbnailHandler(
    IVideoRepository videoRepository,
    IFileRepository fileRepository,
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

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: video.ThumbnailFileId,
            file: command.File,
            publicId: videoId.ToString(),
            folder: "content/video-thumbnails",
            originalFileName: command.File.FileName,
            mimeType: command.File.ContentType,
            cancellationToken: cancellationToken
        );

        video.SetThumbnailFileId(thumbnailFileId: fileEntity.Id);

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUploadVideoThumbnailResult(
            ThumbnailUrl: fileEntity.StorageUrl,
            ThumbnailStorageKey: fileEntity.StorageKey!
        );
    }
}
