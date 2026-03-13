using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a long-form video production on the platform (116 Music Video, 116 Interview,
/// FlexBeat, Le Focus, BTS, Podcast, etc.). Videos are embedded via YouTube — a YouTube ID
/// must be attached before the video can be published.
/// <para>
/// The production workflow is: Draft → submit → PendingPayment/PendingReview → approve →
/// Approved → attach YouTube ID (thumbnail auto-downloaded) → publish → Published.
/// </para>
/// <para>
/// <c>social_boost</c>, <c>is_featured</c>, and <c>featured_until</c> are stamped
/// automatically by the Commerce payment verification flow — never through video endpoints.
/// </para>
/// </summary>
public class VideoEntity : Aggregate<Guid>
{
    /// <summary>
    /// The B2B customer who commissioned this video. <c>null</c> for free content.
    /// </summary>
    public Guid? CustomerId { get; private set; }

    /// <summary>
    /// The order item this video is fulfilling. <c>null</c> for free content.
    /// Both <c>CustomerId</c> and <c>OrderItemId</c> are set together or both are null.
    /// </summary>
    public Guid? OrderItemId { get; private set; }

    /// <summary>
    /// The category this video belongs to (e.g., "116 Le Focus", "116 Music Video").
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// The identity user UUID who authored/hosts this video.
    /// Read from JWT <c>HttpContext.User</c> claims — never passed by the client.
    /// No FK to the identity schema by design (cross-schema FK-free for microservice extractability).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxAuthorIdLength)]
    public string AuthorId { get; private set; } = null!;

    /// <summary>
    /// Display the title of the video.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxTitleLength)]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug used in public video URLs. Must be unique across all videos.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSlugLength)]
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// Optional description shown on the video page below the player.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// URL of the video thumbnail image.
    /// Initially set when a YouTube ID is attached (thumbnail auto-downloaded from YouTube
    /// and re-uploaded to Cloudinary). Can be overridden via <c>POST /{id}/thumbnail</c>.
    /// Never stores YouTube CDN URLs directly — the asset is always owned by the platform.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxThumbnailUrlLength)]
    public string? ThumbnailUrl { get; private set; }

    /// <summary>
    /// Provider-agnostic storage identifier for the thumbnail image asset.
    /// Named <c>ThumbnailStorageKey</c> rather than <c>ThumbnailCloudinaryPublicId</c>
    /// to remain CDN-agnostic (Cloudinary public_id, S3 key, and Bunny path all map cleanly).
    /// Used to delete the old thumbnail when it is replaced or the video is hard deleted.
    /// <c>null</c> until a thumbnail is uploaded.
    /// </summary>
    public string? ThumbnailStorageKey { get; private set; }

    /// <summary>
    /// The YouTube video ID (e.g., "dQw4w9WgXcQ"). Required a gate before publishing —
    /// <see cref="Publish" /> throws if this is null.
    /// Attached via <c>PATCH /api/v1/admin/videos/{id}/YouTube</c>.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxYoutubeVideoIdLength)]
    public string? YoutubeVideoId { get; private set; }

    /// <summary>
    /// Whether the video has been flagged for manual Facebook &amp; Instagram promotion.
    /// Stamped by Commerce payment verification — never set through video endpoints.
    /// </summary>
    public bool SocialBoost { get; private set; }

    /// <summary>
    /// Whether this video has an active featured homepage placement.
    /// Stamped by Commerce payment verification — never set through video endpoints.
    /// </summary>
    public bool IsFeatured { get; private set; }

    /// <summary>
    /// When the featured placement expires. <c>null</c> if not featured.
    /// </summary>
    public DateTimeOffset? FeaturedUntil { get; private set; }

    /// <summary>
    /// Whether a lyrics page is linked to this video.
    /// </summary>
    public bool HasLyrics { get; private set; }

    /// <summary>
    /// Current status in the editorial workflow.
    /// </summary>
    public EnumContentStatus Status { get; private set; }

    /// <summary>
    /// Reason provided when the video is rejected.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxRejectionReasonLength)]
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Scheduled shooting date. Used for pre-booked productions where the client
    /// pays before the shoot takes place.
    /// </summary>
    public DateTimeOffset? ShootingScheduledAt { get; private set; }

    /// <summary>
    /// When the video was first published. <c>null</c> until <c>Publish()</c> is called.
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
    /// Cached average star rating (1–5). Recomputed after each rating event.
    /// </summary>
    public decimal RatingAverage { get; private set; }

    /// <summary>
    /// Total number of ratings received.
    /// </summary>
    public int RatingCount { get; private set; }

    /// <summary>
    /// Cached share count.
    /// </summary>
    public int ShareCount { get; private set; }

    /// <summary>
    /// The customer who commissioned this video. <c>null</c> for free content.
    /// </summary>
    public CustomerEntity? Customer { get; private set; }

    /// <summary>
    /// The category this video belongs to.
    /// </summary>
    public CategoryEntity Category { get; private set; } = null!;

    /// <summary>
    /// Tags applied to this video for discovery and SEO.
    /// </summary>
    public ICollection<VideoTagEntity> Tags { get; } = new List<VideoTagEntity>();

    /// <summary>
    /// Short video teasers linked to this full video.
    /// </summary>
    public ICollection<ShortVideoEntity> Shorts { get; } = new List<ShortVideoEntity>();

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private VideoEntity() { }

    /// <summary>
    /// Creates a new free video record.
    /// </summary>
    /// <param name="id">The unique identifier for the video.</param>
    /// <param name="categoryId">The category this video belongs to.</param>
    /// <param name="title">The video title.</param>
    /// <param name="slug">The URL-safe slug.</param>
    /// <param name="authorId">The identity user UUID from JWT claims.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new <see cref="VideoEntity" /> in <c>Draft</c> status.</returns>
    public static VideoEntity CreateFree(
        Guid id,
        Guid categoryId,
        string title,
        string slug,
        string authorId,
        string? description = null
    )
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw VideoErrors.TitleRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw VideoErrors.SlugRequired();
        }

        return new VideoEntity
        {
            Id = id,
            CategoryId = categoryId,
            Title = title,
            Slug = slug,
            AuthorId = authorId,
            Description = description,
            Status = EnumContentStatus.Draft,
        };
    }

    /// <summary>
    /// Creates a new paid video record linked to a customer and order item.
    /// </summary>
    /// <param name="id">The unique identifier for the video.</param>
    /// <param name="customerId">The B2B customer who commissioned this video.</param>
    /// <param name="orderItemId">The order item this video fulfils.</param>
    /// <param name="categoryId">The category this video belongs to.</param>
    /// <param name="title">The video title.</param>
    /// <param name="slug">The URL-safe slug.</param>
    /// <param name="authorId">The identity user UUID from JWT claims.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new <see cref="VideoEntity" /> in <c>Draft</c> status.</returns>
    public static VideoEntity CreatePaid(
        Guid id,
        Guid customerId,
        Guid orderItemId,
        Guid categoryId,
        string title,
        string slug,
        string authorId,
        string? description = null
    )
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw VideoErrors.TitleRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw VideoErrors.SlugRequired();
        }

        return new VideoEntity
        {
            Id = id,
            CustomerId = customerId,
            OrderItemId = orderItemId,
            CategoryId = categoryId,
            Title = title,
            Slug = slug,
            AuthorId = authorId,
            Description = description,
            Status = EnumContentStatus.Draft,
        };
    }

    /// <summary>
    /// Updates the video's editable metadata fields. Only permitted when status is
    /// <c>Draft</c> or <c>Rejected</c>.
    /// </summary>
    public void UpdateDetails(string title, string slug, string? description)
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw VideoErrors.TitleRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw VideoErrors.SlugRequired();
        }

        Title = title;
        Slug = slug;
        Description = description;
    }

    /// <summary>
    /// Sets or replaces the video thumbnail.
    /// Called when a thumbnail is uploaded via <c>POST /admin/videos/{id}/thumbnail</c>
    /// or automatically after YouTube ID attachment (thumbnail downloaded and re-uploaded).
    /// <para>
    /// <c>ThumbnailStorageKey</c> is the provider-agnostic key used to delete the old
    /// asset from media storage when the thumbnail is replaced or the video is hard deleted.
    /// </para>
    /// </summary>
    /// <param name="thumbnailUrl">The new publicly accessible thumbnail URL.</param>
    /// <param name="thumbnailStorageKey">The new storage key for the thumbnail asset.</param>
    public void UpdateThumbnail(string thumbnailUrl, string thumbnailStorageKey)
    {
        ThumbnailUrl = thumbnailUrl;
        ThumbnailStorageKey = thumbnailStorageKey;
    }

    /// <summary>
    /// Attaches the YouTube video ID. Sets <c>YoutubeVideoId</c> only — the handler
    /// (<c>AttachYoutubeIdCommandHandler</c>) is responsible for downloading the YouTube
    /// thumbnail and calling <see cref="UpdateThumbnail" /> separately.
    /// Keeping these two concerns separate makes the entity method simple and testable.
    /// </summary>
    /// <param name="youtubeVideoId">The YouTube video ID (e.g., "dQw4w9WgXcQ").</param>
    public void AttachYoutubeId(string youtubeVideoId) => YoutubeVideoId = youtubeVideoId;

    /// <summary>
    /// Records or updates the scheduled shooting date.
    /// </summary>
    public void ScheduleShoot(DateTimeOffset scheduledAt) => ShootingScheduledAt = scheduledAt;

    /// <summary>
    /// Updates the video's SEO metadata.
    /// </summary>
    public void UpdateSeo(string? metaTitle, string? metaDescription)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
    }

    /// <summary>
    /// Flags that a lyrics page has been linked to this video.
    /// </summary>
    public void MarkHasLyrics() => HasLyrics = true;

    /// <summary>
    /// Transitions a paid video from <c>Draft</c> → <c>PendingPayment</c>.
    /// </summary>
    public void Submit() => Status = EnumContentStatus.PendingPayment;

    /// <summary>
    /// Transitions a free video from <c>Draft</c> → <c>PendingReview</c>,
    /// or a paid video from <c>PendingPayment</c> → <c>PendingReview</c> after payment is verified.
    /// </summary>
    public void MarkPendingReview() => Status = EnumContentStatus.PendingReview;

    /// <summary>
    /// Marks the video as editorially approved (→ <c>Approved</c>).
    /// </summary>
    public void Approve() => Status = EnumContentStatus.Approved;

    /// <summary>
    /// Publishes the video. Throws if no YouTube ID has been attached —
    /// enforcing the YouTube gate at the domain level.
    /// </summary>
    /// <exception cref="Exception">Thrown when <c>YoutubeVideoId</c> is null or empty.</exception>
    public void Publish()
    {
        if (string.IsNullOrWhiteSpace(YoutubeVideoId))
        {
            throw VideoErrors.CannotPublishWithoutYoutubeId();
        }

        Status = EnumContentStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Rejects the video with a mandatory reason.
    /// </summary>
    public void Reject(string reason)
    {
        Status = EnumContentStatus.Rejected;
        RejectionReason = reason;
    }

    /// <summary>
    /// Archives the video, removing it from all public feeds without deleting it.
    /// Archiving is reversible — Cloudinary thumbnail is <b>not</b> deleted.
    /// </summary>
    public void Archive() => Status = EnumContentStatus.Archived;

    /// <summary>
    /// Flags for manual social media promotion. Called by Commerce only.
    /// </summary>
    public void StampSocialBoost() => SocialBoost = true;

    /// <summary>
    /// Activates featured placement. Called by Commerce only.
    /// </summary>
    public void StampFeatured(DateTimeOffset until)
    {
        IsFeatured = true;
        FeaturedUntil = until;
    }

    /// <summary>
    /// Recomputes the cached rating. Called after each rating insert or update.
    /// </summary>
    public void UpdateRating(decimal average, int count)
    {
        RatingAverage = average;
        RatingCount = count;
    }

    /// <summary>
    /// Increments the cached share count.
    /// </summary>
    public void IncrementShareCount() => ShareCount++;
}
