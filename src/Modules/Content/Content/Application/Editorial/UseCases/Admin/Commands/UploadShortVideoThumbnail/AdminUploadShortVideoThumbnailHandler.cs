using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Handles the <see cref="AdminUploadShortVideoThumbnailCommand" /> to upload or replace a short video thumbnail.
/// The thumbnail file is tracked via <see cref="FileReferenceDto" /> in the Core module.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadShortVideoThumbnailHandler(
    IShortVideoRepository shortVideoRepository,
    IFileStorageService fileStorage,
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

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: command.File,
            publicId: shortVideoId.ToString(),
            folder: "content/short-video-thumbnails",
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileStorage.RecordAsync(
                    file: uploaded,
                    supersededFileId: shortVideo.ThumbnailFileId,
                    cancellationToken: ct
                );

                shortVideo.SetThumbnailFileId(thumbnailFileId: uploaded.Reference.Id);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadShortVideoThumbnailResult(
            ThumbnailUrl: uploaded.Reference.StorageUrl,
            ThumbnailStorageKey: uploaded.Reference.StorageKey!
        );
    }
}
