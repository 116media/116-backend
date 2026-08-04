using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover;

/// <summary>
/// Handles the <see cref="AdminUploadAlbumCoverCommand" /> to upload or replace an album's
/// cover art image. The cover file is tracked via <see cref="FileEntity" /> in the Core module.
/// </summary>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUploadAlbumCoverHandler(
    IAlbumRepository albumRepository,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
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

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: album.CoverImageFileId,
            file: file,
            publicId: command.AlbumId.ToString(),
            folder: "content/album-covers",
            originalFileName: file.FileName,
            mimeType: file.ContentType,
            cancellationToken: cancellationToken
        );

        album.Update(
            name: album.Name,
            coverImageFileId: fileEntity.Id,
            releaseYear: album.ReleaseYear,
            label: album.Label,
            releaseType: album.ReleaseType,
            errors: i18n.Album
        );

        albumRepository.Update(album: album);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUploadAlbumCoverResult(
            CoverImageUrl: fileEntity.StorageUrl,
            CoverImageStorageKey: fileEntity.StorageKey!
        );
    }
}
