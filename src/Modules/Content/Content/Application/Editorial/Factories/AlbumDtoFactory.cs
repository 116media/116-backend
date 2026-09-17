using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Factory implementation building album projections from a pre-resolved cover map.
/// </summary>
/// <param name="fileStorage">Core's storage contract.</param>
public class AlbumDtoFactory(IFileStorageService fileStorage) : IAlbumDtoFactory
{
    /// <inheritdoc />
    public async Task<AlbumDto> CreateAsync(AlbumEntity album, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> covers = await ResolveCoversAsync([album], ct);

        return album.ToAlbumDto(coverImageUrl: CoverUrl(album, covers));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AlbumDto>> CreateManyAsync(
        IReadOnlyList<AlbumEntity> albums,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> covers = await ResolveCoversAsync(albums, ct);

        return albums.Select(album => album.ToAlbumDto(coverImageUrl: CoverUrl(album, covers))).ToList();
    }

    /// <summary>
    /// Resolves every distinct cover the supplied albums reference, in one query.
    /// </summary>
    /// <param name="albums">The albums whose covers to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The covers, keyed by file id.</returns>
    private Task<IReadOnlyDictionary<Guid, FileReferenceDto>> ResolveCoversAsync(
        IReadOnlyList<AlbumEntity> albums,
        CancellationToken ct
    )
    {
        return fileStorage.ResolveManyAsync(
            albums.Where(a => a.CoverImageFileId.HasValue).Select(a => a.CoverImageFileId!.Value).Distinct().ToList(),
            ct
        );
    }

    /// <summary>
    /// Reads an album's cover URL out of the resolved map.
    /// </summary>
    /// <param name="album">The album.</param>
    /// <param name="covers">The resolved covers, keyed by file id.</param>
    /// <returns>The URL, or null when the album has no cover.</returns>
    private static string? CoverUrl(AlbumEntity album, IReadOnlyDictionary<Guid, FileReferenceDto> covers) =>
        album.CoverImageFileId.HasValue ? covers.GetValueOrDefault(album.CoverImageFileId.Value)?.StorageUrl : null;
}
