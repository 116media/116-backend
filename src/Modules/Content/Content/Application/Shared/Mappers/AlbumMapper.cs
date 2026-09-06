using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapping extensions for the <see cref="AlbumEntity" /> domain entity.
/// </summary>
public static class AlbumMapper
{
    /// <summary>
    /// Maps an <see cref="AlbumEntity" /> to an <see cref="AlbumDto" />,
    /// resolving the cover image URL from the associated FileEntity.
    /// </summary>
    public static async Task<AlbumDto> ToAlbumDtoAsync(
        this AlbumEntity entity,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileRepository, ct);

        return entity.ToAlbumDto(coverImageUrl: coverImageUrl);
    }

    /// <summary>
    /// Maps an <see cref="AlbumEntity" /> to an <see cref="AlbumDto" /> from an already
    /// resolved cover URL. Performs no IO — the batch list mapping resolves files up front.
    /// </summary>
    public static AlbumDto ToAlbumDto(this AlbumEntity entity, string? coverImageUrl)
    {
        return new AlbumDto(
            entity.Id,
            entity.Name,
            entity.ArtistId,
            coverImageUrl,
            entity.ReleaseYear,
            entity.Label,
            entity.ReleaseType
        );
    }

    /// <summary>
    /// Maps a list of <see cref="AlbumEntity" /> to a list of <see cref="AlbumDto" />,
    /// resolving cover image URLs from associated FileEntity records.
    /// </summary>
    public static async Task<IReadOnlyList<AlbumDto>> ToAlbumDtosAsync(
        this IReadOnlyList<AlbumEntity> entities,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileEntity> files = await fileRepository.GetByIdsAsync(
            entities.Where(e => e.CoverImageFileId.HasValue).Select(e => e.CoverImageFileId!.Value).Distinct().ToList(),
            ct
        );

        return entities
            .Select(entity =>
                entity.ToAlbumDto(
                    coverImageUrl: entity.CoverImageFileId.HasValue
                        ? files.GetValueOrDefault(entity.CoverImageFileId.Value)?.StorageUrl
                        : null
                )
            )
            .ToList();
    }

    /// <summary>
    /// Resolves the cover image URL for an album. Returns null when no cover has been
    /// uploaded, mirroring <see cref="LyricsMapper" />'s equivalent resolution helper.
    /// </summary>
    private static async Task<string?> ResolveCoverImageUrlAsync(
        AlbumEntity entity,
        IFileRepository fileRepository,
        CancellationToken ct
    )
    {
        if (!entity.CoverImageFileId.HasValue)
        {
            return null;
        }

        FileEntity? coverFile = await fileRepository.GetByIdAsync(entity.CoverImageFileId.Value, ct);
        return coverFile?.StorageUrl;
    }
}
