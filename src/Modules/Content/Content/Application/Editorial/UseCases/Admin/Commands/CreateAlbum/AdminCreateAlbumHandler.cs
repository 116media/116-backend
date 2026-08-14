using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum;

/// <summary>
/// Handles the <see cref="AdminCreateAlbumCommand" /> to create a new album.
/// </summary>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateAlbumHandler(
    IAlbumRepository albumRepository,
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    ContentI18n i18n
) : ICommandHandler<AdminCreateAlbumCommand, AdminCreateAlbumResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateAlbumResult> Handle(
        AdminCreateAlbumCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.ArtistId.HasValue)
        {
            await artistRepository.GetByIdOrThrowAsync(
                id: command.ArtistId.Value,
                cancellationToken: cancellationToken
            );
        }

        AlbumEntity album = AlbumEntity.Create(
            id: Guid.NewGuid(),
            name: command.Name,
            artistId: command.ArtistId,
            coverImageFileId: null,
            releaseYear: command.ReleaseYear,
            label: command.Label,
            releaseType: command.ReleaseType,
            errors: i18n.Album
        );

        await albumRepository.AddAsync(album: album, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = await album.ToAlbumDtoAsync(fileRepository, cancellationToken);
        return new AdminCreateAlbumResult(Album: dto);
    }
}
