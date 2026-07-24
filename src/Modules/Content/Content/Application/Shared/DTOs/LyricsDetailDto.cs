using _116.Content.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a single lyrics page detail view.
/// Extends the summary with the full lyrics text, commerce, and editorial workflow fields.
/// </summary>
/// <param name="Id">
/// The unique identifier of the lyrics record.
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
/// The name of the artist.
/// </param>
/// <param name="Slug">
/// The URL-safe slug used in public lyrics URLs.
/// </param>
/// <param name="LyricsText">
/// The full lyrics text.
/// </param>
/// <param name="Language">
/// The ISO-639 language code of the lyrics (e.g., "fr", "en").
/// </param>
/// <param name="VideoId">
/// The linked video identifier, or null if not linked.
/// </param>
/// <param name="Status">
/// The current editorial workflow status.
/// </param>
/// <param name="RejectionReason">
/// The rejection reason, if the lyrics page was rejected.
/// </param>
/// <param name="PublishedAt">
/// When the lyrics page was published, or null if not yet published.
/// </param>
/// <param name="MetaTitle">
/// Custom SEO meta title, or null.
/// </param>
/// <param name="MetaDescription">
/// Custom SEO meta description, or null.
/// </param>
/// <param name="CoverImageUrl">
/// The publicly accessible URL of the cover/album art image, resolved from the associated
/// FileEntity. Null if no cover image has been uploaded.
/// </param>
/// <param name="Album">
/// The album this song appears on, or null if unknown.
/// </param>
/// <param name="ReleaseYear">
/// The year the song was released, or null if unknown.
/// </param>
/// <param name="Label">
/// The record label that released the song, or null if unknown.
/// </param>
/// <param name="Songwriter">
/// The credited songwriter, or null if unknown or not distinct from the performer.
/// </param>
/// <param name="Producer">
/// The credited producer, or null if unknown or not distinct from the performer.
/// </param>
/// <param name="Tags">
/// Tags applied to this lyrics page for discovery and similar-lyrics matching.
/// </param>
/// <param name="AuthorId">
/// The identity user UUID of the author.
/// </param>
/// <param name="CustomerId">
/// The B2B customer UUID this lyrics page was commissioned for, or null for free content.
/// </param>
/// <param name="CustomerName">
/// The full name of the commissioning customer, or null for free content.
/// </param>
/// <param name="OrderItemId">
/// The order item UUID this lyrics page is linked to, or null for free content.
/// </param>
/// <param name="Author">
/// The resolved author profile, or null when listing.
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
public record LyricsDetailDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string SongTitle,
    string ArtistName,
    string Slug,
    string LyricsText,
    string Language,
    Guid? VideoId,
    EnumContentStatus Status,
    string? RejectionReason,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    string? CoverImageUrl,
    string? Album,
    short? ReleaseYear,
    string? Label,
    string? Songwriter,
    string? Producer,
    IReadOnlyList<TagDto> Tags,
    string AuthorId,
    int ViewCount,
    int LikeCount,
    int ShareCount,
    Guid? CustomerId = null,
    string? CustomerName = null,
    Guid? OrderItemId = null,
    AuthorDto? Author = null,
    bool IsLiked = false
) : AuditableDto;
