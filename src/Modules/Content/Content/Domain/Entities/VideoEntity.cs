using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
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
/// <c>social_boost</c>, <c>is_promoted</c>, and <c>promoted_until</c> are stamped
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
    public Guid AuthorId { get; private set; }

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
    /// Description shown on the video page below the player.
    /// </summary>
    public string Description { get; private set; } = null!;

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
    /// The full YouTube video URL (e.g., "https://www.youtube.com/watch?v=dQw4w9WgXcQ").
    /// Required as a gate before publishing — <see cref="Publish" /> throws if this is null.
    /// Attached via <c>PATCH /api/v1/admin/videos/{id}/YouTube</c>.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxYoutubeVideoUrlLength)]
    public string? YoutubeVideoUrl { get; private set; }

    /// <summary>
    /// Whether the video has been flagged for manual Facebook &amp; Instagram promotion.
    /// Stamped by Commerce payment verification — never set through video endpoints.
    /// </summary>
    public bool SocialBoost { get; private set; }

    /// <summary>
    /// Whether this video has an active paid promotion placement.
    /// Stamped by Commerce payment verification — never set through video endpoints.
    /// </summary>
    public bool IsPromoted { get; private set; }

    /// <summary>
    /// The promotion level purchased for this video's homepage placement.
    /// Determines which grid spot the video appears in (<see cref="PromotionLevelEntity.SpotPriority"/>).
    /// <c>null</c> if the video has never been promoted.
    /// </summary>
    public Guid? PromotionLevelId { get; private set; }

    /// <summary>
    /// Navigation property to the promotion level entity.
    /// </summary>
    public PromotionLevelEntity? PromotionLevel { get; private set; }

    /// <summary>
    /// When the paid promotion expires. <c>null</c> if not promoted.
    /// Set to <c>payment.verified_at + promotion_level.duration_days</c> by the Commerce flow.
    /// </summary>
    public DateTimeOffset? PromotedUntil { get; private set; }

    /// <summary>
    /// When a SuperAdmin force-unpromoted this video. <c>null</c> if never force-unpromoted.
    /// </summary>
    public DateTimeOffset? UnpromotedAt { get; private set; }

    /// <summary>
    /// Identity of the SuperAdmin who applied the force-unpromote. <c>null</c> if never force-unpromoted.
    /// </summary>
    public string? UnpromotedBy { get; private set; }

    /// <summary>
    /// Reason recorded when a SuperAdmin force-unpromoted this video (max 500 chars).
    /// Used as evidence for future refund processing.
    /// </summary>
    [MaxLength(500)]
    public string? UnpromotedReason { get; private set; }

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
    /// <param name="description">The description.</param>
    /// <returns>A new <see cref="VideoEntity" /> in <c>Draft</c> status.</returns>
    public static VideoEntity CreateFree(
        Guid id,
        Guid categoryId,
        string title,
        string slug,
        Guid authorId,
        string description
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
    /// <param name="description">The description.</param>
    /// <returns>A new <see cref="VideoEntity" /> in <c>Draft</c> status.</returns>
    public static VideoEntity CreatePaid(
        Guid id,
        Guid customerId,
        Guid orderItemId,
        Guid categoryId,
        string title,
        string slug,
        Guid authorId,
        string description
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
    /// Updates all editable video fields in a single call. Permitted when status is
    /// <c>Draft</c>, <c>PendingPayment</c>, <c>PendingReview</c>, or <c>Rejected</c>.
    /// </summary>
    public void Update(
        Guid categoryId,
        string title,
        string slug,
        string description,
        Guid? customerId,
        Guid? orderItemId,
        bool socialBoost,
        string? metaTitle,
        string? metaDescription
    )
    {
        CategoryId = categoryId;
        Title = title;
        Slug = slug;
        Description = description;
        CustomerId = customerId;
        OrderItemId = orderItemId;
        SocialBoost = socialBoost;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
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
    /// Attaches the full YouTube video URL. Sets <c>YoutubeVideoUrl</c> only — the handler
    /// (<c>AttachYoutubeVideoUrlCommandHandler</c>) is responsible for downloading the YouTube
    /// thumbnail and calling <see cref="UpdateThumbnail" /> separately.
    /// Keeping these two concerns separate makes the entity method simple and testable.
    /// </summary>
    /// <param name="youtubeVideoUrl">
    /// The full YouTube video URL (e.g., "https://www.youtube.com/watch?v=dQw4w9WgXcQ").
    /// </param>
    /// <exception cref="BadRequestException">
    /// Thrown when a shooting is scheduled in the future, meaning the video has not yet been shot.
    /// </exception>
    public void AttachYoutubeVideoUrl(string youtubeVideoUrl)
    {
        if (ShootingScheduledAt.HasValue && ShootingScheduledAt.Value > DateTimeOffset.UtcNow)
        {
            throw VideoErrors.CannotAttachYoutubeUrlBeforeShoot(ShootingScheduledAt.Value);
        }

        YoutubeVideoUrl = youtubeVideoUrl;
    }

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
    /// Clears the lyrics link flag when no lyrics page references this video.
    /// </summary>
    public void UnmarkHasLyrics() => HasLyrics = false;

    /// <summary>
    /// Transitions a paid video from <c>Draft</c> → <c>PendingPayment</c>.
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
    /// Transitions a free video from <c>Draft</c> → <c>PendingReview</c>,
    /// or a paid video from <c>PendingPayment</c> → <c>PendingReview</c> after payment is verified.
    /// </summary>
    /// <returns><c>true</c> if moved to pending review; <c>false</c> if already pending review.</returns>
    public bool MarkPendingReview()
    {
        if (Status == EnumContentStatus.PendingReview)
        {
            return false;
        }

        Status = EnumContentStatus.PendingReview;
        return true;
    }

    /// <summary>
    /// Marks the video as editorially approved (→ <c>Approved</c>).
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
    /// Publishes the video. Throws if no YouTube ID has been attached —
    /// enforcing the YouTube gate at the domain level.
    /// </summary>
    /// <returns><c>true</c> if published; <c>false</c> if already published.</returns>
    /// <exception cref="Exception">Thrown when <c>YoutubeVideoUrl</c> is null or empty.</exception>
    public bool Publish()
    {
        if (Status == EnumContentStatus.Published)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(YoutubeVideoUrl))
        {
            throw VideoErrors.CannotPublishWithoutYoutubeUrl();
        }

        Status = EnumContentStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
        return true;
    }

    /// <summary>
    /// Rejects the video with a mandatory reason.
    /// </summary>
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
    /// Archives the video, removing it from all public feeds without deleting it.
    /// Archiving is reversible — Cloudinary thumbnail is <b>not</b> deleted.
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
    /// Flags for manual social media promotion. Called by Commerce only.
    /// </summary>
    public void StampSocialBoost() => SocialBoost = true;

    /// <summary>
    /// Activates the video's paid promotion placement until the given date.
    /// Called by the Commerce payment verification flow only.
    /// </summary>
    /// <param name="promotionLevelId">
    /// The promotion level purchased, used to determine the homepage grid spot.
    /// </param>
    /// <param name="until">
    /// When the promotion expires (<c>payment.verified_at + promotion_level.duration_days</c>).
    /// </param>
    public void StampPromotion(Guid promotionLevelId, DateTimeOffset until)
    {
        IsPromoted = true;
        PromotionLevelId = promotionLevelId;
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
    /// Thrown when the video does not have an active promotion.
    /// </exception>
    public void ForceUnpromote(string unpromotedBy, string reason)
    {
        if (!IsPromoted)
        {
            throw new BadRequestException("Video is not currently promoted.");
        }

        IsPromoted = false;
        PromotedUntil = null;
        UnpromotedAt = DateTimeOffset.UtcNow;
        UnpromotedBy = unpromotedBy;
        UnpromotedReason = reason;
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
