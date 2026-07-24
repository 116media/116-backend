using _116.Content.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a lyrics page in list and feed views.
/// Contains the fields needed to render a lyrics card, excluding the full lyrics text.
/// </summary>
/// <param name="Id">
/// The unique identifier of the lyrics page.
/// </param>
/// <param name="CategoryId">
/// The identifier of the lyrics page's category.
/// </param>
/// <param name="CategoryName">
/// The display name of the lyrics page's category.
/// </param>
/// <param name="SongTitle">
/// The title of the song.
/// </param>
/// <param name="ArtistName">
/// The name of the performing artist.
/// </param>
/// <param name="Slug">
/// The URL-safe slug used in public lyrics URLs.
/// </param>
/// <param name="Language">
/// The ISO 639-1 language code of the lyrics (e.g., "fr", "en").
/// </param>
/// <param name="VideoId">
/// The linked video identifier, or null if not linked.
/// </param>
/// <param name="CoverImageUrl">
/// The publicly accessible URL of the cover/album art image, resolved from the associated
/// FileEntity. Null if no cover image has been uploaded.
/// </param>
/// <param name="AuthorId">
/// The identity user UUID of the author.
/// </param>
/// <param name="Status">
/// The current editorial workflow status.
/// </param>
/// <param name="PublishedAt">
/// When the lyrics page was published, or null if not yet published.
/// </param>
/// <param name="ViewCount">
/// Cached number of views. Incremented by interaction events.
/// </param>
/// <param name="LikeCount">
/// Cached number of likes. Incremented and decremented by interaction events.
/// </param>
/// <param name="ShareCount">
/// Cached number of shares. Incremented by interaction events.
/// </param>
/// <param name="IsLiked">
/// Whether the current authenticated user has liked this lyrics page. False for anonymous
/// requests and for authenticated users who have not liked it.
/// </param>
public record LyricsSummaryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string SongTitle,
    string ArtistName,
    string Slug,
    string Language,
    Guid? VideoId,
    string? CoverImageUrl,
    string AuthorId,
    EnumContentStatus Status,
    DateTimeOffset? PublishedAt,
    int ViewCount,
    int LikeCount,
    int ShareCount,
    bool IsLiked = false
) : AuditableDto;
