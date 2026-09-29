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
/// Represents a written editorial article on the platform — artist profiles, reviews, gossip,
/// sponsored spotlights, lyrics pages, and other written content formats.
/// <para>
/// Article creation is a two-step process:
/// <list type="number">
///   <item>Step 1 ("Save Draft"): <c>POST /api/v1/admin/articles</c> creates the shell with
///   identifiers only. <c>body</c> and <c>headline</c> default to empty strings.
///   Returns <c>articleId</c> which the frontend uses for image uploads.</item>
///   <item>Step 2 ("Submit"): <c>PUT /api/v1/admin/articles/{id}</c> saves content,
///   then <c>PATCH /{id}/submit</c> transitions the status.</item>
/// </list>
/// </para>
/// <para>
/// <c>social_boost</c>, <c>is_promoted</c>, and <c>promoted_until</c> are stamped
/// automatically by the Commerce payment verification flow — never through article endpoints.
/// </para>
/// </summary>
public partial class ArticleEntity : Aggregate<Guid>
{
    /// <summary>
    /// The B2B customer who commissioned this article. <c>null</c> for free editorial content.
    /// </summary>
    public Guid? CustomerId { get; private set; }

    /// <summary>
    /// The order item this article is fulfilling. <c>null</c> for free content.
    /// Both <c>CustomerId</c> and <c>OrderItemId</c> are set together or both are null.
    /// </summary>
    public Guid? OrderItemId { get; private set; }

    /// <summary>
    /// The category this article belongs to (e.g., "Artist Profile", "Chronique Sale").
    /// Determines whether the article is free or paid and carries the pricing configuration.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Display the title of the article.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxTitleLength)]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug used in public article URLs (e.g., "fally-ipupa-album-review-2025").
    /// Must be unique across all articles.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSlugLength)]
    public Slug Slug { get; private set; } = null!;

    /// <summary>
    /// Short teaser or aperçu displayed on article cards, feeds, and meta's previews.
    /// Between 100 and 300 characters. Empty string on draft creation (step 1);
    /// minimum length enforced at the application layer on <c>PUT</c> (step 2).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxHeadlineLength)]
    public string Headline { get; private set; } = string.Empty;

    /// <summary>
    /// Rich-text HTML body of the article. Empty string on draft creation (step 1);
    /// filled in step 2 via <c>PUT /api/v1/admin/articles/{id}</c>.
    /// All image URLs in the body are Cloudinary URLs — no base64 or blob URLs.
    /// </summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// ID of the uploaded cover image file tracked in the Core module.
    /// References a FileEntity in the Core module.
    /// </summary>
    public Guid? CoverImageFileId { get; private set; }

    /// <summary>
    /// The identity user UUID who authored this article.
    /// Read from JWT <c>HttpContext.User</c> claims in the handler — never passed by the client.
    /// Stored as a plain string with no FK to the identity schema by design:
    /// the content schema is cross-schema FK-free so it can be extracted as a microservice.
    /// Distinguished from <c>CreatedBy</c> (system audit trail) — <c>AuthorId</c> is the
    /// public editorial byline ("Written by…").
    /// </summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// Whether the article has been flagged for manual Facebook &amp; Instagram promotion.
    /// Stamped by Commerce payment verification — never set through article endpoints.
    /// </summary>
    public bool SocialBoost { get; private set; }

    /// <summary>
    /// Whether this article has an active paid promotion placement (À-la-Une).
    /// Stamped by Commerce payment verification — never set through article endpoints.
    /// </summary>
    public bool IsPromoted { get; private set; }

    /// <summary>
    /// The promotion level purchased for this article's homepage placement.
    /// Determines which grid spot the article appears in (<see cref="PromotionLevelEntity.SpotPriority"/>).
    /// <c>null</c> if the article has never been promoted.
    /// </summary>
    public Guid? PromotionLevelId { get; private set; }

    /// <summary>
    /// When the paid promotion expires. <c>null</c> if not promoted.
    /// Set to <c>payment.verified_at + promotion_level.duration_days</c> by the Commerce flow.
    /// </summary>
    public DateTimeOffset? PromotedUntil { get; private set; }

    /// <summary>
    /// When a SuperAdmin force-unpromoted this article. <c>null</c> if never force-unpromoted.
    /// </summary>
    public DateTimeOffset? UnpromotedAt { get; private set; }

    /// <summary>
    /// Identity of the SuperAdmin who applied the force-unpromote. <c>null</c> if never force-unpromoted.
    /// </summary>
    public string? UnpromotedBy { get; private set; }

    /// <summary>
    /// Reason recorded when a SuperAdmin force-unpromoted this article (max 500 chars).
    /// Used as evidence for future refund processing.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxUnpromotedReasonLength)]
    public string? UnpromotedReason { get; private set; }

    /// <summary>
    /// Current status in the editorial workflow.
    /// </summary>
    public EnumContentStatus Status { get; private set; }

    /// <summary>
    /// Reason provided by the editorial team when rejecting the article.
    /// Set by <c>Reject(reason)</c>; cleared on next submission.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxRejectionReasonLength)]
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// When the article was first published. <c>null</c> until <c>Publish()</c> is called.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; private set; }

    /// <summary>
    /// Custom SEO meta title (max 70 chars). Falls back to <c>Title</c> if null.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxMetaTitleLength)]
    public string? MetaTitle { get; private set; }

    /// <summary>
    /// Custom SEO meta description (max 160 chars).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxMetaDescriptionLength)]
    public string? MetaDescription { get; private set; }

    // Maintained by application-level event handlers — not by DB triggers.

    /// <summary>
    /// Cached like count, maintained by <c>ArticleInteractionRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int LikeCount { get; private init; }

    /// <summary>
    /// Cached comment count, maintained by <c>ArticleInteractionRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int CommentCount { get; private init; }

    /// <summary>
    /// Cached share count, maintained by <c>ArticleInteractionRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int ShareCount { get; private init; }

    /// <summary>
    /// Cached bookmark count, maintained by <c>ArticleInteractionRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int BookmarkCount { get; private init; }

    /// <summary>
    /// All image assets associated with this article (cover + body images).
    /// Used by the image diff algorithm on update and by the hard-delete handler
    /// to clean up Cloudinary assets before the DB delete.
    /// </summary>
    public ICollection<ArticleImageEntity> Images { get; } = new List<ArticleImageEntity>();

    /// <summary>
    /// Tags applied to this article for discovery and SEO.
    /// </summary>
    public ICollection<ArticleTagEntity> Tags { get; } = new List<ArticleTagEntity>();

    /// <summary>
    /// The artists credited on this article, one junction row per artist. Written only through
    /// <see cref="ReplaceArtists" />.
    /// </summary>
    public ICollection<ArticleArtistEntity> Artists { get; } = new List<ArticleArtistEntity>();

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ArticleEntity() { }

    /// <summary>
    /// Creates a new free article shell (step 1 — "Save Draft").
    /// <c>body</c> and <c>headline</c> default to empty strings and are filled in step 2.
    /// </summary>
    /// <param name="id">The unique identifier for the article.</param>
    /// <param name="categoryId">The category this article belongs to.</param>
    /// <param name="title">The article title.</param>
    /// <param name="slug">The URL-safe slug.</param>
    /// <param name="authorId">The identity user UUID from JWT claims.</param>
    /// <returns>A new <see cref="ArticleEntity" /> in <c>Draft</c> status.</returns>
    public static ArticleEntity CreateFree(Guid id, Guid categoryId, string title, string slug, Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw new ContentRuleException(ContentRuleCodes.ArticleTitleRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.ArticleSlugRequired);
        }

        return new ArticleEntity
        {
            Id = id,
            CategoryId = categoryId,
            Title = title,
            Slug = slug,
            AuthorId = authorId,
            Status = EnumContentStatus.Draft,
        };
    }

    /// <summary>
    /// Creates a new paid article shell (step 1 — "Save Draft") linked to a customer and order item.
    /// Both <paramref name="customerId" /> and <paramref name="orderItemId" /> must be provided together.
    /// </summary>
    /// <param name="id">The unique identifier for the article.</param>
    /// <param name="customerId">The B2B customer who commissioned this article.</param>
    /// <param name="orderItemId">The order item this article fulfils.</param>
    /// <param name="categoryId">The category this article belongs to.</param>
    /// <param name="title">The article title.</param>
    /// <param name="slug">The URL-safe slug.</param>
    /// <param name="authorId">The identity user UUID from JWT claims.</param>
    /// <returns>A new <see cref="ArticleEntity" /> in <c>Draft</c> status.</returns>
    public static ArticleEntity CreatePaid(
        Guid id,
        Guid customerId,
        Guid orderItemId,
        Guid categoryId,
        string title,
        string slug,
        Guid authorId
    )
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw new ContentRuleException(ContentRuleCodes.ArticleTitleRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.ArticleSlugRequired);
        }

        return new ArticleEntity
        {
            Id = id,
            CustomerId = customerId,
            OrderItemId = orderItemId,
            CategoryId = categoryId,
            Title = title,
            Slug = slug,
            AuthorId = authorId,
            Status = EnumContentStatus.Draft,
        };
    }
}
