using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
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
public partial class VideoEntity : Aggregate<Guid>
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
    public Slug Slug { get; private set; } = null!;

    /// <summary>
    /// Description shown on the video page below the player.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// ID of the uploaded thumbnail file tracked in the Storage module.
    /// The thumbnail URL and storage key are resolved from the associated FileEntity.
    /// </summary>
    public Guid? ThumbnailFileId { get; private set; }

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
    [MaxLength(length: ContentConstants.MaxUnpromotedReasonLength)]
    public string? UnpromotedReason { get; private set; }

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
    /// Cached average star rating (1–5), maintained by <c>VideoRepository.SetRatingAsync</c>.
    /// </summary>
    public decimal RatingAverage { get; private init; }

    /// <summary>
    /// Total number of ratings received, maintained by <c>VideoRepository.SetRatingAsync</c>.
    /// </summary>
    public int RatingCount { get; private init; }

    /// <summary>
    /// Cached share count, maintained by <c>VideoRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int ShareCount { get; private init; }

    /// <summary>
    /// Tags applied to this video for discovery and SEO.
    /// </summary>
    public ICollection<VideoTagEntity> Tags { get; } = new List<VideoTagEntity>();

    /// <summary>
    /// Optional link to a claimed <see cref="ArtistEntity" /> profile. Null for the common case
    /// of an unclaimed artist — the free-text artist name shown on the video page (if any)
    /// remains the display fallback either way.
    /// </summary>
    public Guid? ArtistId { get; private set; }

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
            throw new ContentRuleException(ContentRuleCodes.VideoTitleRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.VideoSlugRequired);
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
            throw new ContentRuleException(ContentRuleCodes.VideoTitleRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.VideoSlugRequired);
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
}
