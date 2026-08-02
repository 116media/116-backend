using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Handles the <see cref="AdminCreateArtistCommand" /> to create a new, unclaimed artist profile.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateArtistHandler(
    IArtistRepository artistRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    ContentI18n i18n
) : ICommandHandler<AdminCreateArtistCommand, AdminCreateArtistResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateArtistResult> Handle(
        AdminCreateArtistCommand command,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity? existing = await artistRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Artist.SlugAlreadyExists(slug: command.Slug);
        }

        ArtistEntity artist = ArtistEntity.Create(
            id: Guid.NewGuid(),
            name: command.Name,
            slug: command.Slug,
            bio: command.Bio,
            errors: i18n.Artist
        );

        await artistRepository.AddAsync(artist: artist, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = await artist.ToArtistDtoAsync(fileRepository, cancellationToken);
        return new AdminCreateArtistResult(Artist: dto);
    }
}
