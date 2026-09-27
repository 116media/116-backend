using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a song lyrics page on the platform. Lyrics pages are SEO-optimised standalone
/// pages that target keyword searches (e.g., "Fally Ipupa — Eloko Oyo lyrics").
/// <para>
/// A lyrics entry can be:
/// <list type="bullet">
///   <item>Linked to a <see cref="VideoEntity" /> (e.g., a lyric video or "Behind the Lyrics" episode)</item>
///   <item>Standalone with no parent content</item>
/// </list>
/// </para>
/// <para>
/// Follows the same slug/category/commerce/editorial-status model as <see cref="ArticleEntity" />:
/// a lyrics page is created either free (<see cref="CreateFree" />) or paid on behalf of a
/// customer's order item (<see cref="CreatePaid" />), then moves through the editorial workflow
/// via <see cref="Submit" />, <see cref="MarkPendingReview" />, <see cref="Approve" />,
/// <see cref="Publish" />, <see cref="Reject" />, and <see cref="Archive" />.
/// </para>
/// </summary>
public partial class LyricsEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the admin who created this lyrics page.
    /// No FK to the identity schema by design.
    /// </summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// The B2B customer who commissioned this lyrics page. <c>null</c> for free editorial content.
    /// </summary>
    public Guid? CustomerId { get; private set; }

    /// <summary>
    /// The order item this lyrics page is fulfilling. <c>null</c> for free content.
    /// Both <c>CustomerId</c> and <c>OrderItemId</c> are set together or both are null.
    /// </summary>
    public Guid? OrderItemId { get; private set; }

    /// <summary>
    /// The category this lyrics page belongs to. Determines whether the page is free or paid
    /// and carries the pricing configuration.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// URL-safe slug used in public lyrics URLs (e.g., "fally-ipupa-eloko-oyo-lyrics").
    /// Must be unique across all lyrics pages.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSlugLength)]
    public Slug Slug { get; private set; } = null!;

    /// <summary>
    /// Optional link to a parent video. <c>null</c> unless this lyrics page is
    /// associated with a lyric video or a "Behind the Lyrics" episode.
    /// </summary>
    public Guid? VideoId { get; private set; }

    /// <summary>
    /// The title of the song.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSongTitleLength)]
    public string SongTitle { get; private set; } = null!;

    /// <summary>
    /// The name of the performing artist.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxArtistNameLength)]
    public string ArtistName { get; private set; } = null!;

    /// <summary>
    /// The full lyrics text of the song.
    /// </summary>
    public string LyricsText { get; private set; } = null!;

    /// <summary>
    /// ISO 639-1 language code (e.g., "fr", "ln", "en"). BCP-47 subtags supported (e.g., "fr-CD").
    /// Defaults to "fr" for Lingala/French content.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxLyricsLanguageLength)]
    public string Language { get; private set; } = ContentConstants.DefaultLyricsLanguage;

    /// <summary>
    /// Current status in the editorial workflow.
    /// </summary>
    public EnumContentStatus Status { get; private set; }

    /// <summary>
    /// Reason provided by the editorial team when rejecting the lyrics page.
    /// Set by <c>Reject(reason)</c>; cleared on next submission.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxRejectionReasonLength)]
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// When the lyrics page was first published. <c>null</c> until <c>Publish()</c> is called.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; private set; }

    /// <summary>
    /// Custom SEO meta title (max 70 chars).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxMetaTitleLength)]
    public string? MetaTitle { get; private set; }

    /// <summary>
    /// Custom SEO meta description (max 160 chars).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxMetaDescriptionLength)]
    public string? MetaDescription { get; private set; }

    /// <summary>
    /// Schema.org JSON-LD structured data for enhanced Google search results.
    /// Stored as JSONB in PostgreSQL. Generated automatically or provided manually.
    /// </summary>
    public string? StructuredData { get; private set; }

    /// <summary>
    /// ID of the uploaded cover/album art file tracked in the Core module. Null until an
    /// admin uploads one. The cover image URL is resolved from the associated FileEntity.
    /// </summary>
    public Guid? CoverImageFileId { get; private set; }

    /// <summary>
    /// The album this song appears on, if known.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxAlbumNameLength)]
    public string? Album { get; private set; }

    /// <summary>
    /// The year the song was released, if known.
    /// </summary>
    public short? ReleaseYear { get; private set; }

    /// <summary>
    /// The record label that released the song, if known.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxLabelNameLength)]
    public string? Label { get; private set; }

    /// <summary>
    /// The credited songwriter, if distinct from and known separately to the performer.
    /// Distinct from <see cref="AuthorId" />, which is CMS attribution, not a song credit.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCreditNameLength)]
    public string? Songwriter { get; private set; }

    /// <summary>
    /// The credited producer, if distinct from and known separately to the performer.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCreditNameLength)]
    public string? Producer { get; private set; }

    /// <summary>
    /// Optional link to a claimed <see cref="ArtistEntity" /> profile. Null for the common case
    /// of an unclaimed artist — <see cref="ArtistName" /> remains the display fallback either way.
    /// Coexists with <see cref="ArtistName" />; it never replaces it.
    /// </summary>
    public Guid? ArtistId { get; private set; }

    /// <summary>
    /// Optional link to a real <see cref="AlbumEntity" /> row. Null for the common case of an
    /// unlinked album — <see cref="Album" /> remains the display fallback either way. Coexists
    /// with <see cref="Album" />; it never replaces it.
    /// </summary>
    public Guid? AlbumId { get; private set; }

    /// <summary>
    /// Whether this lyrics page has an active paid promotion placement.
    /// Stamped by Commerce payment verification — never set through lyrics endpoints.
    /// </summary>
    public bool IsPromoted { get; private set; }

    /// <summary>
    /// When the paid promotion expires. <c>null</c> if not promoted.
    /// Set to <c>payment.verified_at + promotion_level.duration_days</c> by the Commerce flow.
    /// </summary>
    public DateTimeOffset? PromotedUntil { get; private set; }

    /// <summary>
    /// When a SuperAdmin force-unpromoted this lyrics page. <c>null</c> if never force-unpromoted.
    /// </summary>
    public DateTimeOffset? UnpromotedAt { get; private set; }

    /// <summary>
    /// Identity of the SuperAdmin who applied the force-unpromote. <c>null</c> if never force-unpromoted.
    /// </summary>
    public string? UnpromotedBy { get; private set; }

    /// <summary>
    /// Reason recorded when a SuperAdmin force-unpromoted this lyrics page (max 500 chars).
    /// Used as evidence for future refund processing.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxUnpromotedReasonLength)]
    public string? UnpromotedReason { get; private set; }

    // Maintained by application-level event handlers — not by DB triggers.

    /// <summary>
    /// Cached view count, maintained by <c>LyricsRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int ViewCount { get; private init; }

    /// <summary>
    /// Cached like count, maintained by <c>LyricsRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int LikeCount { get; private init; }

    /// <summary>
    /// Cached share count, maintained by <c>LyricsRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int ShareCount { get; private init; }

    /// <summary>
    /// Tags applied to this lyrics page for discovery and similar-lyrics matching.
    /// </summary>
    public ICollection<LyricsTagEntity> Tags { get; } = new List<LyricsTagEntity>();

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private LyricsEntity() { }
}
