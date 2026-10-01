using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Resolves and applies an artist creation for the admin create-artist use case.
/// </summary>
/// <param name="artistRepository">Repository checking the slug and staging the artist.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
/// <param name="timeProvider">Clock the birthdate guard reads today from.</param>
public class AdminCreateArtistService(IArtistRepository artistRepository, ContentI18n i18n, TimeProvider timeProvider)
    : IAdminCreateArtistService
{
    /// <inheritdoc />
    public async Task<ArtistEntity> CreateAsync(AdminCreateArtistCommand command, CancellationToken cancellationToken)
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
            realName: command.RealName,
            aliases: command.Aliases,
            birthdate: command.Birthdate,
            hometown: command.Hometown,
            today: DateOnly.FromDateTime(dateTime: timeProvider.GetUtcNow().UtcDateTime)
        );

        await artistRepository.AddAsync(artist: artist, cancellationToken: cancellationToken);

        return artist;
    }
}
