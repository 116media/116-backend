using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
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
public class LyricsEntity : Aggregate<Guid>
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
    public string Slug { get; private set; } = null!;

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
    [MaxLength(500)]
    public string? UnpromotedReason { get; private set; }

    // Maintained by application-level event handlers — not by DB triggers.

    /// <summary>
    /// Cached view count. Incremented by interaction events.
    /// </summary>
    public int ViewCount { get; private set; }

    /// <summary>
    /// Cached like count. Incremented/decremented by interaction events.
    /// </summary>
    public int LikeCount { get; private set; }

    /// <summary>
    /// Cached share count. Incremented by interaction events.
    /// </summary>
    public int ShareCount { get; private set; }

    /// <summary>
    /// Tags applied to this lyrics page for discovery and similar-lyrics matching.
    /// </summary>
    public ICollection<LyricsTagEntity> Tags { get; } = new List<LyricsTagEntity>();

    /// <summary>
    /// The parent video this lyrics page is linked to. <c>null</c> if standalone.
    /// </summary>
    public VideoEntity? Video { get; private set; }

    /// <summary>
    /// The customer who commissioned this lyrics page. <c>null</c> for free content.
    /// </summary>
    public CustomerEntity? Customer { get; private set; }

    /// <summary>
    /// The category this lyrics page belongs to.
    /// </summary>
    public CategoryEntity Category { get; private set; } = null!;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private LyricsEntity() { }

    /// <summary>
    /// Creates a new free lyrics page.
    /// </summary>
    /// <param name="id">The unique identifier for the lyrics page.</param>
    /// <param name="categoryId">The category this lyrics page belongs to.</param>
    /// <param name="videoId">Optional linked video UUID.</param>
    /// <param name="songTitle">The song title.</param>
    /// <param name="artistName">The performing artist name.</param>
    /// <param name="lyricsText">The full lyrics text.</param>
    /// <param name="language">ISO 639-1 language code.</param>
    /// <param name="slug">The URL-safe slug.</param>
    /// <param name="authorId">The identity user UUID from JWT claims.</param>
    /// <param name="errors">The errors factory instance.</param>
    /// <returns>A new <see cref="LyricsEntity" /> in <c>Draft</c> status.</returns>
    public static LyricsEntity CreateFree(
        Guid id,
        Guid categoryId,
        Guid? videoId,
        string songTitle,
        string artistName,
        string lyricsText,
        string language,
        string slug,
        Guid authorId,
        LyricsErrors errors
    )
    {
        ValidateRequiredFields(songTitle: songTitle, artistName: artistName, lyricsText: lyricsText, errors: errors);

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw errors.SlugRequired();
        }

        return new LyricsEntity
        {
            Id = id,
            CategoryId = categoryId,
            VideoId = videoId,
            SongTitle = songTitle,
            ArtistName = artistName,
            LyricsText = lyricsText,
            Language = language,
            Slug = slug,
            AuthorId = authorId,
            Status = EnumContentStatus.Draft,
        };
    }

    /// <summary>
    /// Creates a new paid lyrics page linked to a customer and order item.
    /// Both <paramref name="customerId" /> and <paramref name="orderItemId" /> must be provided together.
    /// </summary>
    /// <param name="id">The unique identifier for the lyrics page.</param>
    /// <param name="customerId">The B2B customer who commissioned this lyrics page.</param>
    /// <param name="orderItemId">The order item this lyrics page fulfils.</param>
    /// <param name="categoryId">The category this lyrics page belongs to.</param>
    /// <param name="videoId">Optional linked video UUID.</param>
    /// <param name="songTitle">The song title.</param>
    /// <param name="artistName">The performing artist name.</param>
    /// <param name="lyricsText">The full lyrics text.</param>
    /// <param name="language">ISO 639-1 language code.</param>
    /// <param name="slug">The URL-safe slug.</param>
    /// <param name="authorId">The identity user UUID from JWT claims.</param>
    /// <param name="errors">The errors factory instance.</param>
    /// <returns>A new <see cref="LyricsEntity" /> in <c>Draft</c> status.</returns>
    public static LyricsEntity CreatePaid(
        Guid id,
        Guid customerId,
        Guid orderItemId,
        Guid categoryId,
        Guid? videoId,
        string songTitle,
        string artistName,
        string lyricsText,
        string language,
        string slug,
        Guid authorId,
        LyricsErrors errors
    )
    {
        ValidateRequiredFields(songTitle: songTitle, artistName: artistName, lyricsText: lyricsText, errors: errors);

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw errors.SlugRequired();
        }

        return new LyricsEntity
        {
            Id = id,
            CustomerId = customerId,
            OrderItemId = orderItemId,
            CategoryId = categoryId,
            VideoId = videoId,
            SongTitle = songTitle,
            ArtistName = artistName,
            LyricsText = lyricsText,
            Language = language,
            Slug = slug,
            AuthorId = authorId,
            Status = EnumContentStatus.Draft,
        };
    }

    /// <summary>
    /// Updates the lyrics content and metadata fields in a single call.
    /// The status gate, slug uniqueness, and category existence are enforced at the
    /// application layer by the update handler, not here.
    /// </summary>
    /// <param name="categoryId">
    /// The category this lyrics page belongs to.
    /// </param>
    /// <param name="songTitle">
    /// The song title.
    /// </param>
    /// <param name="artistName">
    /// The performing artist name.
    /// </param>
    /// <param name="slug">
    /// The URL-safe slug. Uniqueness enforced by handler.
    /// </param>
    /// <param name="lyricsText">
    /// The full lyrics text.
    /// </param>
    /// <param name="language">
    /// ISO 639-1 language code.
    /// </param>
    /// <param name="videoId">
    /// Optional linked video UUID.
    /// </param>
    /// <param name="customerId">
    /// The B2B customer who commissioned this lyrics page. <c>null</c> for free content.
    /// </param>
    /// <param name="orderItemId">
    /// The order item this lyrics page fulfils. <c>null</c> for free content.
    /// </param>
    /// <param name="errors">
    /// The errors factory instance.
    /// </param>
    public void Update(
        Guid categoryId,
        string songTitle,
        string artistName,
        string slug,
        string lyricsText,
        string language,
        Guid? videoId,
        Guid? customerId,
        Guid? orderItemId,
        LyricsErrors errors
    )
    {
        ValidateRequiredFields(songTitle, artistName, lyricsText, errors);

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw errors.SlugRequired();
        }

        CategoryId = categoryId;
        SongTitle = songTitle;
        ArtistName = artistName;
        Slug = slug;
        LyricsText = lyricsText;
        Language = language;
        VideoId = videoId;
        CustomerId = customerId;
        OrderItemId = orderItemId;
    }

    /// <summary>
    /// Updates the SEO metadata for this lyrics page.
    /// </summary>
    public void UpdateSeo(string? metaTitle, string? metaDescription, string? structuredData)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        StructuredData = structuredData;
    }

    /// <summary>
    /// Updates the song-credit fields. Each parameter is independently optional — passing
    /// null for one field clears only that field, leaving the others untouched.
    /// </summary>
    /// <param name="album">
    /// The album name, or null to clear.
    /// </param>
    /// <param name="releaseYear">
    /// The release year, or null to clear.
    /// </param>
    /// <param name="label">
    /// The record label, or null to clear.
    /// </param>
    /// <param name="songwriter">
    /// The credited songwriter, or null to clear.
    /// </param>
    /// <param name="producer">
    /// The credited producer, or null to clear.
    /// </param>
    public void UpdateMetadata(string? album, short? releaseYear, string? label, string? songwriter, string? producer)
    {
        Album = album;
        ReleaseYear = releaseYear;
        Label = label;
        Songwriter = songwriter;
        Producer = producer;
    }

    /// <summary>
    /// Sets or clears the cover/album art file reference.
    /// </summary>
    /// <param name="coverImageFileId">
    /// The FileEntity ID, or null to clear it.
    /// </param>
    public void SetCoverImageFileId(Guid? coverImageFileId)
    {
        CoverImageFileId = coverImageFileId;
    }

    /// <summary>
    /// Transitions a paid lyrics page from <c>Draft</c> → <c>PendingPayment</c>.
    /// Call <see cref="MarkPendingReview" /> instead for free lyrics pages.
    /// </summary>
    /// <returns><c>true</c> if submitted; <c>false</c> if already pending payment.</returns>
    public bool Submit()
    {
        if (Status == EnumContentStatus.PendingPayment)
        {
            return false;
        }

        Status = EnumContentStatus.PendingPayment;
        return true;
    }

    /// <summary>
    /// Transitions a free lyrics page from <c>Draft</c> → <c>PendingReview</c>,
    /// or a paid lyrics page from <c>PendingPayment</c> → <c>PendingReview</c> after payment is verified.
    /// A no-op when the page is already <c>PendingReview</c> or already <c>Published</c> — the
    /// latter is what makes retroactive promotion safe: buying promoted placement on an
    /// already-live page (spec 12) must stamp <see cref="StampPromotion" /> without silently
    /// un-publishing it back into the review queue.
    /// </summary>
    /// <returns>
    /// <c>true</c> if moved to pending review; <c>false</c> if already pending review or already published.
    /// </returns>
    public bool MarkPendingReview()
    {
        if (Status is EnumContentStatus.PendingReview or EnumContentStatus.Published)
        {
            return false;
        }

        Status = EnumContentStatus.PendingReview;
        return true;
    }

    /// <summary>
    /// Marks the lyrics page as editorially approved (→ <c>Approved</c>).
    /// </summary>
    /// <returns><c>true</c> if approved; <c>false</c> if already approved.</returns>
    public bool Approve()
    {
        if (Status == EnumContentStatus.Approved)
        {
            return false;
        }

        Status = EnumContentStatus.Approved;
        return true;
    }

    /// <summary>
    /// Publishes the lyrics page and records the publication timestamp.
    /// </summary>
    /// <returns><c>true</c> if published; <c>false</c> if already published.</returns>
    public bool Publish()
    {
        if (Status == EnumContentStatus.Published)
        {
            return false;
        }

        Status = EnumContentStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
        return true;
    }

    /// <summary>
    /// Rejects the lyrics page with a mandatory reason and transitions to <c>Rejected</c>.
    /// The admin can revise the lyrics page and resubmit it.
    /// </summary>
    /// <param name="reason">The rejection reason visible to the editorial team.</param>
    /// <returns><c>true</c> if rejected; <c>false</c> if already rejected.</returns>
    public bool Reject(string reason)
    {
        if (Status == EnumContentStatus.Rejected)
        {
            return false;
        }

        Status = EnumContentStatus.Rejected;
        RejectionReason = reason;
        return true;
    }

    /// <summary>
    /// Archives the lyrics page, removing it from all public feeds without deleting it.
    /// Archiving is reversible.
    /// </summary>
    /// <returns><c>true</c> if archived; <c>false</c> if already archived.</returns>
    public bool Archive()
    {
        if (Status == EnumContentStatus.Archived)
        {
            return false;
        }

        Status = EnumContentStatus.Archived;
        return true;
    }

    /// <summary>
    /// Links this record to a claimed artist profile. Coexists with, and does not clear,
    /// the free-text <see cref="ArtistName" /> field.
    /// </summary>
    /// <param name="artistId">The <see cref="ArtistEntity" /> ID to link.</param>
    public void LinkArtist(Guid artistId) => ArtistId = artistId;

    /// <summary>
    /// Clears the artist profile link, reverting display to the plain-text
    /// <see cref="ArtistName" /> only.
    /// </summary>
    public void UnlinkArtist() => ArtistId = null;

    /// <summary>
    /// Links this record to a real album. Coexists with, and does not clear, the free-text
    /// <see cref="Album" /> field.
    /// </summary>
    /// <param name="albumId">The <see cref="AlbumEntity" /> ID to link.</param>
    public void LinkAlbum(Guid albumId) => AlbumId = albumId;

    /// <summary>
    /// Clears the album link, reverting display to the plain-text <see cref="Album" /> only.
    /// </summary>
    public void UnlinkAlbum() => AlbumId = null;

    /// <summary>
    /// Activates the lyrics page's paid promotion placement until the given date.
    /// Called by the Commerce payment verification flow only.
    /// </summary>
    /// <param name="promotionLevelId">
    /// The promotion level purchased. Accepted for signature parity with the Commerce
    /// verification call site — not persisted on the entity, mirroring
    /// <see cref="ArticleEntity.StampPromotion" />; the purchased level lives on
    /// <see cref="ContentOrderItemEntity" /> and is consulted at verification time only.
    /// </param>
    /// <param name="until">
    /// When the promotion expires (<c>payment.verified_at + promotion_level.duration_days</c>).
    /// </param>
    public void StampPromotion(Guid promotionLevelId, DateTimeOffset until)
    {
        IsPromoted = true;
        PromotedUntil = until;
    }

    /// <summary>
    /// Force-removes the active paid promotion. SuperAdmin only.
    /// Records the audit trail needed for future pro-rata refund calculation.
    /// </summary>
    /// <param name="unpromotedBy">
    /// Identity of the SuperAdmin performing the force-unpromote, read from JWT claims.
    /// </param>
    /// <param name="reason">
    /// Mandatory reason for the force-unpromote (e.g. "government request", "policy violation").
    /// </param>
    /// <exception cref="BadRequestException">
    /// Thrown when the lyrics page does not have an active promotion.
    /// </exception>
    public void ForceUnpromote(string unpromotedBy, string reason)
    {
        if (!IsPromoted)
        {
            throw new BadRequestException("Lyrics page is not currently promoted.");
        }

        IsPromoted = false;
        PromotedUntil = null;
        UnpromotedAt = DateTimeOffset.UtcNow;
        UnpromotedBy = unpromotedBy;
        UnpromotedReason = reason;
    }

    /// <summary>
    /// Increments the cached view count.
    /// </summary>
    public void IncrementViewCount() => ViewCount++;

    /// <summary>
    /// Increments the cached like count.
    /// </summary>
    public void IncrementLikeCount() => LikeCount++;

    /// <summary>
    /// Decrements the cached like count, floor at zero.
    /// </summary>
    public void DecrementLikeCount() => LikeCount = Math.Max(0, LikeCount - 1);

    /// <summary>
    /// Increments the cached share count.
    /// </summary>
    public void IncrementShareCount() => ShareCount++;

    /// <summary>
    /// Replaces the canonical lyrics text — used when a community correction is accepted.
    /// Distinct from the full <see cref="Update" /> call, which requires re-supplying every field.
    /// </summary>
    public void ReplaceLyricsText(string lyricsText) => LyricsText = lyricsText;

    private static void ValidateRequiredFields(
        string songTitle,
        string artistName,
        string lyricsText,
        LyricsErrors errors
    )
    {
        if (string.IsNullOrWhiteSpace(value: songTitle))
        {
            throw errors.SongTitleRequired();
        }

        if (string.IsNullOrWhiteSpace(value: artistName))
        {
            throw errors.ArtistNameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: lyricsText))
        {
            throw errors.LyricsTextRequired();
        }
    }
}
