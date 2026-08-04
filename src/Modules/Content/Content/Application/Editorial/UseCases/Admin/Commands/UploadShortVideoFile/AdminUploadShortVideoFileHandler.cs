using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;

/// <summary>
/// Handles the <see cref="AdminUploadShortVideoFileCommand" /> to upload or replace a short video's file.
/// The video file is tracked via <see cref="FileEntity" /> in the Core module. Uploading a file to a
/// draft makes it eligible for activation.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadShortVideoFileHandler(
    IShortVideoRepository shortVideoRepository,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminUploadShortVideoFileCommand, AdminUploadShortVideoFileResult>
{
    /// <inheritdoc />
    public async Task<AdminUploadShortVideoFileResult> Handle(
        AdminUploadShortVideoFileCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid shortVideoId = Guid.Parse(command.ShortVideoId);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: shortVideoId,
            cancellationToken: cancellationToken
        );

        IFormFile file = command.File!;

        FileEntity fileEntity = await fileRepository.ReplaceVideoFileAsync(
            currentFileId: shortVideo.VideoFileId,
            file: file,
            publicId: shortVideoId.ToString(),
            folder: "content/short-videos",
            originalFileName: file.FileName,
            mimeType: file.ContentType,
            cancellationToken: cancellationToken
        );

        shortVideo.ReplaceVideoFile(videoFileId: fileEntity.Id);

        shortVideoRepository.Update(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUploadShortVideoFileResult(
            VideoUrl: fileEntity.StorageUrl,
            VideoStorageKey: fileEntity.StorageKey!
        );
    }
}
