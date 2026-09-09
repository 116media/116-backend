using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapping extensions for the <see cref="ArtistEntity" /> domain entity.
/// </summary>
public static class ArtistMapper
{
    /// <summary>
    /// Maps an <see cref="ArtistEntity" /> to an <see cref="ArtistDto" />, resolving the
    /// avatar URL from the associated FileEntity. The verified flag is derived here from the
    /// claim state — the claiming user's identity never leaves the entity.
    /// </summary>
    public static async Task<ArtistDto> ToArtistDtoAsync(
        this ArtistEntity entity,
        IFileStorageService fileStorage,
        CancellationToken ct = default,
        IReadOnlyList<ArtistSocialLinkEntity>? socialLinks = null
    )
    {
        string? avatarUrl = await ResolveAvatarUrlAsync(entity, fileStorage, ct);

        return entity.ToArtistDto(avatarUrl: avatarUrl, socialLinks: socialLinks);
    }

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

    /// <summary>
    /// Maps a list of <see cref="ArtistEntity" /> to a list of <see cref="ArtistDto" />,
    /// resolving avatar URLs from associated FileReferenceDto records.
    /// </summary>
    public static async Task<IReadOnlyList<ArtistDto>> ToArtistDtosAsync(
        this IReadOnlyList<ArtistEntity> entities,
        IFileStorageService fileStorage,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> files = await fileStorage.ResolveManyAsync(
            entities.Where(e => e.AvatarFileId.HasValue).Select(e => e.AvatarFileId!.Value).Distinct().ToList(),
            ct
        );

        return entities
            .Select(entity =>
                entity.ToArtistDto(
                    avatarUrl: entity.AvatarFileId.HasValue
                        ? files.GetValueOrDefault(entity.AvatarFileId.Value)?.StorageUrl
                        : null
                )
            )
            .ToList();
    }

    /// <summary>
    /// Resolves the avatar URL for an artist profile. Returns null when no avatar has been
    /// uploaded, mirroring <see cref="LyricsMapper" />'s equivalent resolution helper.
    /// </summary>
    private static async Task<string?> ResolveAvatarUrlAsync(
        ArtistEntity entity,
        IFileStorageService fileStorage,
        CancellationToken ct
    )
    {
        if (!entity.AvatarFileId.HasValue)
        {
            return null;
        }

        FileReferenceDto? avatarFile = await fileStorage.ResolveAsync(entity.AvatarFileId.Value, ct);
        return avatarFile?.StorageUrl;
    }
}
