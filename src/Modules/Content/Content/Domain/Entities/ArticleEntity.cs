using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
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
/// <c>social_boost</c>, <c>is_featured</c>, and <c>featured_until</c> are stamped
/// automatically by the Commerce payment verification flow — never through article endpoints.
/// </para>
/// </summary>
public class ArticleEntity : Aggregate<Guid>
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
    /// Display title of the article.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxTitleLength)]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug used in public article URLs (e.g., "fally-ipupa-album-review-2025").
    /// Must be unique across all articles.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSlugLength)]
    public string Slug { get; private set; } = null!;

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
    /// URL of the article's primary cover image. Tracked in <c>article_images</c>
    /// alongside all body images so it can be cleaned up on update or delete.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCoverImageUrlLength)]
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// The identity user UUID who authored this article.
    /// Read from JWT <c>HttpContext.User</c> claims in the handler — never passed by the client.
    /// Stored as a plain string with no FK to the identity schema by design:
    /// the content schema is cross-schema FK-free so it can be extracted as a microservice.
    /// Distinguished from <c>CreatedBy</c> (system audit trail) — <c>AuthorId</c> is the
    /// public editorial byline ("Written by…").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxAuthorIdLength)]
    public string AuthorId { get; private set; } = null!;

    /// <summary>
    /// Whether the article has been flagged for manual Facebook &amp; Instagram promotion.
    /// Stamped by Commerce payment verification — never set through article endpoints.
    /// </summary>
    public bool SocialBoost { get; private set; }

    /// <summary>
    /// Whether this article has an active featured/À-la-Une homepage placement.
    /// Stamped by Commerce payment verification — never set through article endpoints.
    /// </summary>
    public bool IsFeatured { get; private set; }

    /// <summary>
    /// When the featured placement expires. <c>null</c> if not featured.
    /// Set to <c>payment.verified_at + promotion_level.duration_days</c> by the Commerce flow.
    /// </summary>
    public DateTimeOffset? FeaturedUntil { get; private set; }

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
    /// Cached like count. Incremented/decremented by interaction events.
    /// </summary>
    public int LikeCount { get; private set; }

    /// <summary>
    /// Cached comment count. Incremented/decremented by interaction events.
    /// </summary>
    public int CommentCount { get; private set; }

    /// <summary>
    /// Cached share count. Incremented by interaction events.
    /// </summary>
    public int ShareCount { get; private set; }

    /// <summary>
    /// Cached bookmark count. Incremented/decremented by interaction events.
    /// </summary>
    public int BookmarkCount { get; private set; }

    /// <summary>
    /// The customer who commissioned this article. <c>null</c> for free content.
    /// </summary>
    public CustomerEntity? Customer { get; private set; }

    /// <summary>
    /// The category this article belongs to.
    /// </summary>
    public CategoryEntity Category { get; private set; } = null!;

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
    public static ArticleEntity CreateFree(Guid id, Guid categoryId, string title, string slug, string authorId)
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw ArticleErrors.TitleRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw ArticleErrors.SlugRequired();
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
        string authorId
    )
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw ArticleErrors.TitleRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw ArticleErrors.SlugRequired();
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

    /// <summary>
    /// Saves the article content (step 2). Called by <c>UpdateArticleCommandHandler</c>
    /// when the admin fills in the body and headline before clicking "Submit".
    /// Also used when revising a <c>Rejected</c> article before resubmission.
    /// </summary>
    /// <param name="headline">The short teaser text (100–300 chars; min enforced by validator).</param>
    /// <param name="body">The rich-text HTML body containing only Cloudinary URLs.</param>
    /// <param name="coverImageUrl">Optional URL of the cover image.</param>
    public void UpdateContent(string headline, string body, string? coverImageUrl)
    {
        Headline = headline;
        Body = body;
        CoverImageUrl = coverImageUrl;
    }

    /// <summary>
    /// Updates the article's SEO metadata. Called by <c>UpdateArticleSeoCommandHandler</c>.
    /// </summary>
    public void UpdateSeo(string? metaTitle, string? metaDescription)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
    }

    /// <summary>
    /// Transitions a paid article from <c>Draft</c> → <c>PendingPayment</c>.
    /// Call <see cref="MarkPendingReview" /> instead for free articles.
    /// </summary>
    public void Submit() => Status = EnumContentStatus.PendingPayment;

    /// <summary>
    /// Transitions a free article from <c>Draft</c> → <c>PendingReview</c>,
    /// or a paid article from <c>PendingPayment</c> → <c>PendingReview</c> after payment is verified.
    /// </summary>
    public void MarkPendingReview() => Status = EnumContentStatus.PendingReview;

    /// <summary>
    /// Marks the article as editorially approved (→ <c>Approved</c>).
    /// </summary>
    public void Approve() => Status = EnumContentStatus.Approved;

    /// <summary>
    /// Publishes the article and records the publication timestamp.
    /// </summary>
    public void Publish()
    {
        Status = EnumContentStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Rejects the article with a mandatory reason and transitions to <c>Rejected</c>.
    /// The admin can revise the article and resubmit it.
    /// </summary>
    /// <param name="reason">The rejection reason visible to the editorial team.</param>
    public void Reject(string reason)
    {
        Status = EnumContentStatus.Rejected;
        RejectionReason = reason;
    }

    /// <summary>
    /// Archives the article, removing it from all public feeds without deleting it.
    /// Archiving is reversible — Cloudinary images are <b>not</b> deleted.
    /// </summary>
    public void Archive() => Status = EnumContentStatus.Archived;

    /// <summary>
    /// Flags the article for manual Facebook &amp; Instagram promotion.
    /// Called by the Commerce payment verification flow only.
    /// </summary>
    public void StampSocialBoost() => SocialBoost = true;

    /// <summary>
    /// Activates the article's featured/À-la-Une homepage placement until the given date.
    /// Called by the Commerce payment verification flow only.
    /// </summary>
    /// <param name="until">When the featured placement expires (<c>verified_at + duration_days</c>).</param>
    public void StampFeatured(DateTimeOffset until)
    {
        IsFeatured = true;
        FeaturedUntil = until;
    }

    /// <summary>
    /// Increments the cached like count.
    /// </summary>
    public void IncrementLikeCount() => LikeCount++;

    /// <summary>
    /// Decrements the cached like count, floor at zero.
    /// </summary>
    public void DecrementLikeCount() => LikeCount = Math.Max(0, LikeCount - 1);

    /// <summary>
    /// Increments the cached comment count.
    /// </summary>
    public void IncrementCommentCount() => CommentCount++;

    /// <summary>
    /// Decrements the cached comment count, floor at zero.
    /// </summary>
    public void DecrementCommentCount() => CommentCount = Math.Max(0, CommentCount - 1);

    /// <summary>
    /// Increments the cached share count.
    /// </summary>
    public void IncrementShareCount() => ShareCount++;

    /// <summary>
    /// Increments the cached bookmark count.
    /// </summary>
    public void IncrementBookmarkCount() => BookmarkCount++;

    /// <summary>
    /// Decrements the cached bookmark count, floor at zero.
    /// </summary>
    public void DecrementBookmarkCount() => BookmarkCount = Math.Max(0, BookmarkCount - 1);
}
