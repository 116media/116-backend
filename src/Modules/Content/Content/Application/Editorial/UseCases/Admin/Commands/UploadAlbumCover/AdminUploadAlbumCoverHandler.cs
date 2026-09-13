using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover;

/// <summary>
/// Handles the <see cref="AdminUploadAlbumCoverCommand" /> to upload or replace an album's
/// cover art image. The cover file is tracked via <see cref="FileReferenceDto" /> in the Core module.
/// </summary>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadAlbumCoverHandler(
    IAlbumRepository albumRepository,
    IFileStorageService fileStorage,
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

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: command.AlbumId.ToString(),
            folder: "content/album-covers",
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileStorage.RecordAsync(
                    file: uploaded,
                    supersededFileId: album.CoverImageFileId,
                    cancellationToken: ct
                );

                album.Update(
                    name: album.Name,
                    coverImageFileId: uploaded.Reference.Id,
                    releaseYear: album.ReleaseYear,
                    label: album.Label,
                    releaseType: album.ReleaseType
                );
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadAlbumCoverResult(
            CoverImageUrl: uploaded.Reference.StorageUrl,
            CoverImageStorageKey: uploaded.Reference.StorageKey!
        );
    }
}
