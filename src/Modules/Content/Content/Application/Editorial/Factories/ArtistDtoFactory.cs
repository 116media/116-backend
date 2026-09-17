using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Factory implementation building artist projections from a pre-resolved avatar map.
/// </summary>
/// <param name="fileStorage">Core's storage contract.</param>
public class ArtistDtoFactory(IFileStorageService fileStorage) : IArtistDtoFactory
{
    /// <inheritdoc />
    public async Task<ArtistDto> CreateAsync(
        ArtistEntity artist,
        IReadOnlyList<ArtistSocialLinkEntity>? socialLinks = null,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> avatars = await ResolveAvatarsAsync([artist], ct);

        return artist.ToArtistDto(avatarUrl: AvatarUrl(artist, avatars), socialLinks: socialLinks);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArtistDto>> CreateManyAsync(
        IReadOnlyList<ArtistEntity> artists,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> avatars = await ResolveAvatarsAsync(artists, ct);

        return artists.Select(artist => artist.ToArtistDto(avatarUrl: AvatarUrl(artist, avatars))).ToList();
    }

    /// <summary>
    /// Resolves every distinct avatar the supplied artists reference, in one query.
    /// </summary>
    /// <param name="artists">The artists whose avatars to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The avatars, keyed by file id.</returns>
    private Task<IReadOnlyDictionary<Guid, FileReferenceDto>> ResolveAvatarsAsync(
        IReadOnlyList<ArtistEntity> artists,
        CancellationToken ct
    )
    {
        return fileStorage.ResolveManyAsync(
            artists.Where(a => a.AvatarFileId.HasValue).Select(a => a.AvatarFileId!.Value).Distinct().ToList(),
            ct
        );
    }

    /// <summary>
    /// Reads an artist's avatar URL out of the resolved map.
    /// </summary>
    /// <param name="artist">The artist.</param>
    /// <param name="avatars">The resolved avatars, keyed by file id.</param>
    /// <returns>The URL, or null when the artist has no avatar.</returns>
    private static string? AvatarUrl(ArtistEntity artist, IReadOnlyDictionary<Guid, FileReferenceDto> avatars) =>
        artist.AvatarFileId.HasValue ? avatars.GetValueOrDefault(artist.AvatarFileId.Value)?.StorageUrl : null;
}
