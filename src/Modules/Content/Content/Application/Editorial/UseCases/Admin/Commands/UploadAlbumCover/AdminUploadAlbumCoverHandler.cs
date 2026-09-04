using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover;

/// <summary>
/// Handles the <see cref="AdminUploadAlbumCoverCommand" /> to upload or replace an album's
/// cover art image. The cover file is tracked via <see cref="FileEntity" /> in the Core module.
/// </summary>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="fileUploadService">Uploads and replaces stored assets.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadAlbumCoverHandler(
    IAlbumRepository albumRepository,
    IFileUploadService fileUploadService,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminUploadAlbumCoverCommand, AdminUploadAlbumCoverResult>
{
    /// <inheritdoc />
    public async Task<AdminUploadAlbumCoverResult> Handle(
        AdminUploadAlbumCoverCommand command,
        CancellationToken cancellationToken
    )
    {
        AlbumEntity album = await albumRepository.GetByIdOrThrowAsync(
            id: command.AlbumId,
            cancellationToken: cancellationToken
        );

        IFormFile file = command.File!;

        FileEntity uploaded = await fileUploadService.UploadImageAsync(
            file: file,
            publicId: command.AlbumId.ToString(),
            folder: "content/album-covers",
            originalFileName: file.FileName,
            mimeType: file.ContentType,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileUploadService.RecordAsync(
                    file: uploaded,
                    supersededFileId: album.CoverImageFileId,
                    cancellationToken: ct
                );

                album.Update(
                    name: album.Name,
                    coverImageFileId: uploaded.Id,
                    releaseYear: album.ReleaseYear,
                    label: album.Label,
                    releaseType: album.ReleaseType
                );

                albumRepository.Update(album: album);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadAlbumCoverResult(
            CoverImageUrl: uploaded.StorageUrl,
            CoverImageStorageKey: uploaded.StorageKey!
        );
    }
}
