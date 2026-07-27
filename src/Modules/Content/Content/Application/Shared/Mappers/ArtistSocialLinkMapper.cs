using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapping extensions for the <see cref="ArtistSocialLinkEntity" /> domain entity.
/// </summary>
public static class ArtistSocialLinkMapper
{
    /// <summary>
    /// Maps an <see cref="ArtistSocialLinkEntity" /> to an <see cref="ArtistSocialLinkDto" />.
    /// Identifiers are dropped — the client renders the row and follows the URLs.
    /// </summary>
    /// <param name="entity">The social link entity to map.</param>
    /// <returns>The mapped <see cref="ArtistSocialLinkDto" />.</returns>
    public static ArtistSocialLinkDto ToArtistSocialLinkDto(this ArtistSocialLinkEntity entity)
    {
        return new ArtistSocialLinkDto(Platform: entity.Platform, Url: entity.Url);
    }

    /// <summary>
    /// Maps a list of <see cref="ArtistSocialLinkEntity" /> to DTOs. A null input collapses
    /// to an empty list so the client never handles two shapes for "no links".
    /// </summary>
    /// <param name="entities">The social link entities to map, or null for none.</param>
    /// <returns>The mapped list, empty when there are no links.</returns>
    public static IReadOnlyList<ArtistSocialLinkDto> ToArtistSocialLinkDtoList(
        this IReadOnlyList<ArtistSocialLinkEntity>? entities
    )
    {
        if (entities is null || entities.Count == 0)
        {
            return [];
        }

        var results = new List<ArtistSocialLinkDto>(capacity: entities.Count);

        foreach (ArtistSocialLinkEntity entity in entities)
        {
            results.Add(item: entity.ToArtistSocialLinkDto());
        }

        return results;
    }
}
