using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapping extensions for the <see cref="ArtistEntity" /> domain entity.
/// </summary>
public static class ArtistMapper
{
    /// <summary>
    /// Maps an <see cref="ArtistEntity" /> to an <see cref="ArtistDto" /> from an already
    /// resolved avatar URL. Performs no IO — the batch list mapping resolves files up front.
    /// </summary>
    public static ArtistDto ToArtistDto(
        this ArtistEntity entity,
        string? avatarUrl,
        IReadOnlyList<ArtistSocialLinkEntity>? socialLinks = null
    )
    {
        return new ArtistDto(
            Id: entity.Id,
            Name: entity.Name,
            Slug: entity.Slug,
            Bio: entity.Bio,
            AvatarUrl: avatarUrl,
            IsVerified: entity.UserId is not null && entity.VerifiedAt is not null,
            RealName: entity.RealName,
            Aliases: entity.Aliases,
            Birthdate: entity.Birthdate,
            Hometown: entity.Hometown,
            SocialLinks: socialLinks.ToArtistSocialLinkDtoList()
        );
    }
}
