using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum;

/// <summary>
/// Handles the <see cref="AdminUpdateAlbumCommand" /> to update an album's editable fields.
/// </summary>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateAlbumHandler(
    IAlbumRepository albumRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    ContentI18n i18n
) : ICommandHandler<AdminUpdateAlbumCommand, AdminUpdateAlbumResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateAlbumResult> Handle(
        AdminUpdateAlbumCommand command,
        CancellationToken cancellationToken
    )
    {
        AlbumEntity album = await albumRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        album.Update(
            name: command.Name,
            coverImageFileId: album.CoverImageFileId,
            releaseYear: command.ReleaseYear,
            label: command.Label,
            releaseType: command.ReleaseType
        );

        albumRepository.Update(album: album);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = await album.ToAlbumDtoAsync(fileRepository, cancellationToken);
        return new AdminUpdateAlbumResult(Album: dto);
    }
}
