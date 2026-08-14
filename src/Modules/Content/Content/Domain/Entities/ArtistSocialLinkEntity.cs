using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents an artist's outbound link to their profile on a social platform.
/// <para>
/// One row per <c>(artist, platform)</c>, enforced by a unique index — mirroring the
/// <see cref="StreamingLinkEntity" /> shape rather than N nullable URL columns on the artist
/// row, so adding a platform is an enum member instead of a migration. The URL is
/// display-and-navigate only; it is never parsed for a handle.
/// </para>
/// </summary>
public class ArtistSocialLinkEntity : Aggregate<Guid>
{
    /// <summary>
    /// The artist profile this link belongs to.
    /// </summary>
    public Guid ArtistId { get; private set; }

    /// <summary>
    /// The social platform this link points to. Unique per artist.
    /// </summary>
    public EnumSocialPlatform Platform { get; private set; }

    /// <summary>
    /// The outbound profile URL on that platform. Always https — enforced by the write-side
    /// validator, and re-checked by the client at render.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxStreamingLinkUrlLength)]
    public string Url { get; private set; } = null!;

    /// <summary>
    /// The artist profile this link belongs to.
    /// </summary>
    public ArtistEntity Artist { get; private set; } = null!;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ArtistSocialLinkEntity() { }

    /// <summary>
    /// Creates a new social link for an artist's platform slot.
    /// </summary>
    /// <param name="id">The unique identifier for the social link.</param>
    /// <param name="artistId">The artist profile this link belongs to.</param>
    /// <param name="platform">The social platform this link points to.</param>
    /// <param name="url">The outbound profile URL.</param>
    /// <returns>A new <see cref="ArtistSocialLinkEntity" />.</returns>
    public static ArtistSocialLinkEntity Create(Guid id, Guid artistId, EnumSocialPlatform platform, string url)
    {
        return new ArtistSocialLinkEntity
        {
            Id = id,
            ArtistId = artistId,
            Platform = platform,
            Url = url,
        };
    }

    /// <summary>
    /// Replaces the URL for this platform slot.
    /// </summary>
    /// <param name="url">The new outbound profile URL.</param>
    public void UpdateUrl(string url) => Url = url;
}
