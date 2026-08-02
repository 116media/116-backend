using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapping extensions for the <see cref="ArtistEntity" /> domain entity.
/// </summary>
public static class ArtistMapper
{
    /// <summary>
    /// Maps an <see cref="ArtistEntity" /> to an <see cref="ArtistDto" />,
    /// resolving the avatar URL from the associated FileEntity.
    /// </summary>
    public static async Task<ArtistDto> ToArtistDtoAsync(
        this ArtistEntity entity,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        string? avatarUrl = await ResolveAvatarUrlAsync(entity, fileRepository, ct);

        return new ArtistDto(entity.Id, entity.Name, entity.Slug, entity.Bio, avatarUrl);
    }

    /// <summary>
    /// Maps a list of <see cref="ArtistEntity" /> to a list of <see cref="ArtistDto" />,
    /// resolving avatar URLs from associated FileEntity records.
    /// </summary>
    public static async Task<IReadOnlyList<ArtistDto>> ToArtistDtosAsync(
        this IReadOnlyList<ArtistEntity> entities,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var results = new List<ArtistDto>(entities.Count);
        foreach (ArtistEntity entity in entities)
        {
            results.Add(await entity.ToArtistDtoAsync(fileRepository, ct));
        }
        return results;
    }

    /// <summary>
    /// Resolves the avatar URL for an artist profile. Returns null when no avatar has been
    /// uploaded, mirroring <see cref="LyricsMapper" />'s equivalent resolution helper.
    /// </summary>
    private static async Task<string?> ResolveAvatarUrlAsync(
        ArtistEntity entity,
        IFileRepository fileRepository,
        CancellationToken ct
    )
    {
        if (!entity.AvatarFileId.HasValue)
        {
            return null;
        }

        FileEntity? avatarFile = await fileRepository.GetByIdAsync(entity.AvatarFileId.Value, ct);
        return avatarFile?.StorageUrl;
    }
}
