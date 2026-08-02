using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a curated deep link to a release on a specific streaming platform.
/// <para>
/// A release is either an album (<see cref="AlbumId" /> set) or a standalone single
/// (<see cref="LyricsId" /> set) — exactly one of the two is ever set, enforced by a database
/// check constraint. Absence of a row for a given platform is expected: the public endpoint
/// falls back to a generated search URL when no curated link exists.
/// </para>
/// </summary>
public class StreamingLinkEntity : Aggregate<Guid>
{
    /// <summary>
    /// The album this link belongs to, when the release is an album. Mutually exclusive with
    /// <see cref="LyricsId" />.
    /// </summary>
    public Guid? AlbumId { get; private set; }

    /// <summary>
    /// The standalone single (lyrics page with no <c>AlbumId</c>) this link belongs to.
    /// Mutually exclusive with <see cref="AlbumId" />.
    /// </summary>
    public Guid? LyricsId { get; private set; }

    /// <summary>
    /// The streaming platform this link points to.
    /// </summary>
    public EnumStreamingPlatform Platform { get; private set; }

    /// <summary>
    /// The curated deep link URL for this platform.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxStreamingLinkUrlLength)]
    public string Url { get; private set; } = null!;

    /// <summary>
    /// The album this link belongs to. <c>null</c> when the link targets a standalone single.
    /// </summary>
    public AlbumEntity? Album { get; private set; }

    /// <summary>
    /// The standalone single this link belongs to. <c>null</c> when the link targets an album.
    /// </summary>
    public LyricsEntity? Lyrics { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private StreamingLinkEntity() { }

    /// <summary>
    /// Creates a new streaming link for an album's platform slot.
    /// </summary>
    /// <param name="id">The unique identifier for the streaming link.</param>
    /// <param name="albumId">The album this link belongs to.</param>
    /// <param name="platform">The streaming platform this link points to.</param>
    /// <param name="url">The curated deep link URL.</param>
    /// <returns>A new <see cref="StreamingLinkEntity" /> targeting the given album.</returns>
    public static StreamingLinkEntity ForAlbum(Guid id, Guid albumId, EnumStreamingPlatform platform, string url)
    {
        return new StreamingLinkEntity
        {
            Id = id,
            AlbumId = albumId,
            Platform = platform,
            Url = url,
        };
    }

    /// <summary>
    /// Creates a new streaming link for a standalone single's platform slot.
    /// </summary>
    /// <param name="id">The unique identifier for the streaming link.</param>
    /// <param name="lyricsId">The standalone single (lyrics page) this link belongs to.</param>
    /// <param name="platform">The streaming platform this link points to.</param>
    /// <param name="url">The curated deep link URL.</param>
    /// <returns>A new <see cref="StreamingLinkEntity" /> targeting the given single.</returns>
    public static StreamingLinkEntity ForSingle(Guid id, Guid lyricsId, EnumStreamingPlatform platform, string url)
    {
        return new StreamingLinkEntity
        {
            Id = id,
            LyricsId = lyricsId,
            Platform = platform,
            Url = url,
        };
    }

    /// <summary>
    /// Replaces the curated deep link URL for this platform slot.
    /// </summary>
    /// <param name="url">The new curated deep link URL.</param>
    public void UpdateUrl(string url) => Url = url;
}
