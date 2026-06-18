using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Handles the <see cref="AdminDeleteShortVideoCommand" /> to permanently delete a short video.
/// Loads the associated FileEntity records to obtain their storage keys, deletes the
/// Cloudinary assets via <see cref="IFileService" />, soft-deletes the FileEntity records,
/// and removes the short video from the database.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="fileService">Service for file storage operations including Cloudinary cleanup.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDeleteShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IFileService fileService,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminDeleteShortVideoCommand, AdminDeleteShortVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteShortVideoResult> Handle(
        AdminDeleteShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (shortVideo.ThumbnailFileId.HasValue)
        {
            FileEntity? thumbnailFile = await fileRepository.GetByIdAsync(
                shortVideo.ThumbnailFileId.Value,
                cancellationToken
            );
            if (thumbnailFile?.StorageKey is not null)
            {
                await fileService.DeleteFileAsync(thumbnailFile.StorageKey, cancellationToken);
            }
            await fileRepository.SoftDeleteByIdAsync(
                fileId: shortVideo.ThumbnailFileId.Value,
                cancellationToken: cancellationToken
            );
        }

        FileEntity? videoFile = await fileRepository.GetByIdAsync(shortVideo.VideoFileId, cancellationToken);
        if (videoFile?.StorageKey is not null)
        {
            await fileService.DeleteFileAsync(videoFile.StorageKey, cancellationToken);
        }
        await fileRepository.SoftDeleteByIdAsync(fileId: shortVideo.VideoFileId, cancellationToken: cancellationToken);

        shortVideoRepository.Remove(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteShortVideoResult(IsSuccess: true);
    }
}
