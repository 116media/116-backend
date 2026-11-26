using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.Contracts;
using _116.Content.Application.Shared.Helpers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Resolves and applies an artist creation for the admin create-artist use case.
/// </summary>
/// <param name="artistRepository">Repository checking the slug and staging the artist.</param>
/// <param name="timeProvider">Clock the birthdate guard reads today from.</param>
public class AdminCreateArtistService(IArtistRepository artistRepository, TimeProvider timeProvider)
    : IAdminCreateArtistService
{
    /// <inheritdoc />
    public async Task<ArtistEntity> CreateAsync(AdminCreateArtistCommand command, CancellationToken cancellationToken)
    {
        string slug = await SlugHelper.ToAvailableSlugAsync(
            name: command.Name,
            isTakenAsync: async (candidate, ct) =>
                await artistRepository.GetBySlugAsync(slug: candidate, cancellationToken: ct) is not null,
            cancellationToken: cancellationToken
        );

        ArtistEntity artist = ArtistEntity.Create(
            id: Guid.NewGuid(),
            name: command.Name,
            slug: slug,
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
