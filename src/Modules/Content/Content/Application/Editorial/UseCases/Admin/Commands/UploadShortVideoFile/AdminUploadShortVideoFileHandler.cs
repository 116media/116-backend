using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;

/// <summary>
/// Handles the <see cref="AdminUploadShortVideoFileCommand" /> to upload or replace a short video's file.
/// The video file is tracked via <see cref="FileReferenceDto" /> in the Core module. Uploading a file to a
/// draft makes it eligible for activation.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadShortVideoFileHandler(
    IShortVideoRepository shortVideoRepository,
    IFileStorageService fileStorage,
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

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: shortVideoId.ToString(),
            folder: "content/short-videos",
            kind: EnumStoredFileKind.Video,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileStorage.RecordAsync(
                    file: uploaded,
                    supersededFileId: shortVideo.VideoFileId,
                    cancellationToken: ct
                );

                shortVideo.ReplaceVideoFile(videoFileId: uploaded.Reference.Id);

                shortVideoRepository.Update(shortVideo: shortVideo);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadShortVideoFileResult(
            VideoUrl: uploaded.Reference.StorageUrl,
            VideoStorageKey: uploaded.Reference.StorageKey!
        );
    }
}
