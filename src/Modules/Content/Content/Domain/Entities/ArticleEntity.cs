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

    /// <summary>
    /// Renames the article and its slug. Allowed while the article is editable — <c>Draft</c>,
    /// <c>PendingPayment</c>, <c>PendingReview</c> or <c>Rejected</c>; slug uniqueness is the
    /// handler's to enforce.
    /// </summary>
    /// <param name="title">The article title.</param>
    /// <param name="slug">The URL-safe slug. Uniqueness enforced by handler.</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool Retitle(string title, string slug)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Article);

        if (Title == title && Slug == slug)
        {
            return false;
        }

        Title = title;
        Slug = slug;

        return true;
    }

    /// <summary>
    /// Revises the teaser and the rich-text body, declaring the body images the new body drops.
    /// </summary>
    /// <param name="headline">The short teaser text (100–300 chars; min enforced by validator).</param>
    /// <param name="body">The rich-text HTML body containing only Cloudinary URLs.</param>
    /// <param name="orphanedBodyImageStorageKeys">
    /// Storage keys of body images that drop out of the new body, computed by the handler
    /// against the pre-update image set. When non-empty the revision declares the orphaning
    /// so post-commit consumers can remove the rows and the remote assets.
    /// </param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool ReviseBody(string headline, string body, IReadOnlyList<string>? orphanedBodyImageStorageKeys = null)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Article);

        if (Headline == headline && Body == body)
        {
            return false;
        }

        Headline = headline;
        Body = body;

        if (orphanedBodyImageStorageKeys is { Count: > 0 })
        {
            AddDomainEvent(
                new ArticleBodyImagesOrphanedEvent(ArticleId: Id, StorageKeys: orphanedBodyImageStorageKeys)
            );
        }

        return true;
    }

    /// <summary>
    /// Moves the article to another category. Existence of the category is checked by the handler.
    /// </summary>
    /// <param name="categoryId">The category this article belongs to.</param>
    /// <returns><c>true</c> if the category changed; otherwise <c>false</c>.</returns>
    public bool Recategorize(Guid categoryId)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Article);

        if (CategoryId == categoryId)
        {
            return false;
        }

        CategoryId = categoryId;

        return true;
    }

    /// <summary>
    /// Assigns — or clears — the commission this article fulfils, together with its social boost flag.
    /// </summary>
    /// <param name="customerId">The B2B customer who commissioned this article. <c>null</c> for free content.</param>
    /// <param name="orderItemId">The order item this article fulfils. <c>null</c> for free content.</param>
    /// <param name="socialBoost">Whether this article is flagged for social media promotion.</param>
    /// <returns><c>true</c> if any value changed; otherwise <c>false</c>.</returns>
    public bool AssignCommission(Guid? customerId, Guid? orderItemId, bool socialBoost)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Article);

        if (CustomerId == customerId && OrderItemId == orderItemId && SocialBoost == socialBoost)
        {
            return false;
        }

        CustomerId = customerId;
        OrderItemId = orderItemId;
        SocialBoost = socialBoost;

        return true;
    }

    /// <summary>
    /// Sets the cover image file reference. Called by <c>UploadArticleImageCommandHandler</c>
    /// when a <c>Cover</c>-type image is uploaded.
    /// </summary>
    /// <param name="coverImageFileId">
    /// The FileEntity ID for the uploaded cover image, or null to clear it.
    /// </param>
    public void UpdateCoverImage(Guid? coverImageFileId)
    {
        CoverImageFileId = coverImageFileId;
    }

    /// <summary>
    /// Revises the SEO metadata. Unlike the editorial verbs this is allowed at any status, since
    /// search metadata is maintained after publication.
    /// </summary>
    /// <param name="metaTitle">Optional SEO meta title (max 70 chars). Falls back to <c>Title</c> if null.</param>
    /// <param name="metaDescription">Optional SEO meta description (max 160 chars).</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool ReviseSeo(string? metaTitle, string? metaDescription)
    {
        if (MetaTitle == metaTitle && MetaDescription == metaDescription)
        {
            return false;
        }

        MetaTitle = metaTitle;
        MetaDescription = metaDescription;

        return true;
    }

    /// <summary>
    /// Transitions a paid article from <c>Draft</c> → <c>PendingPayment</c>.
    /// Call <see cref="MarkPendingReview" /> instead for free articles.
    /// </summary>
    /// <returns><c>true</c> if submitted; <c>false</c> if already pending payment.</returns>
    public bool Submit()
    {
        if (Status == EnumContentStatus.PendingPayment)
        {
            return false;
        }

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.PendingPayment,
            contentType: EnumCoreContentType.Article
        );

        Status = EnumContentStatus.PendingPayment;

        return true;
    }

    /// <summary>
    /// Transitions a free article from <c>Draft</c> → <c>PendingReview</c>,
    /// or a paid article from <c>PendingPayment</c> → <c>PendingReview</c> after payment is verified.
    /// Only <c>Draft</c>, <c>PendingPayment</c> and <c>Rejected</c> advance: an article whose
    /// editorial state already moved past review (<c>PendingReview</c>, <c>Approved</c>,
    /// <c>Published</c>, <c>Archived</c>) is left untouched, so a replayed payment effect never
    /// pulls approved or live content back into the review queue and retroactive promotion on an
    /// already-live article stamps <see cref="StampPromotion" /> without un-publishing it.
    /// <c>Rejected</c> advances by design: the revise-and-resubmit flow is how rejected content
    /// re-enters review.
    /// </summary>
    /// <returns>
    /// <c>true</c> if moved to pending review; <c>false</c> when the editorial state is already
    /// at or past review.
    /// </returns>
    public bool MarkPendingReview()
    {
        if (Status is not (EnumContentStatus.Draft or EnumContentStatus.PendingPayment or EnumContentStatus.Rejected))
        {
            return false;
        }

        Status = EnumContentStatus.PendingReview;
        return true;
    }

    /// <summary>
    /// Marks the article as editorially approved (→ <c>Approved</c>).
    /// </summary>
    /// <returns><c>true</c> if approved; <c>false</c> if already approved.</returns>
    public bool Approve()
    {
        if (Status == EnumContentStatus.Approved)
        {
            return false;
        }

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.Approved,
            contentType: EnumCoreContentType.Article
        );

        Status = EnumContentStatus.Approved;
        return true;
    }

    /// <summary>
    /// Publishes the article and records the publication timestamp.
    /// </summary>
    /// <returns><c>true</c> if published; <c>false</c> if already published.</returns>
    public bool Publish(DateTimeOffset now)
    {
        if (Status == EnumContentStatus.Published)
        {
            return false;
        }

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.Published,
            contentType: EnumCoreContentType.Article
        );

        Status = EnumContentStatus.Published;
        PublishedAt = now;

        AddDomainEvent(
            new CommissionedContentPublishedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Article,
                CustomerId: CustomerId,
                Title: Title,
                Slug: Slug
            )
        );
        AddDomainEvent(new ArticlePublishedEvent(ArticleId: Id));

        return true;
    }

    /// <summary>
    /// Rejects the article with a mandatory reason and transitions to <c>Rejected</c>.
    /// The admin can revise the article and resubmit it.
    /// </summary>
    /// <param name="reason">The rejection reason visible to the editorial team.</param>
    /// <returns><c>true</c> if rejected; <c>false</c> if already rejected.</returns>
    public bool Reject(string reason)
    {
        if (Status == EnumContentStatus.Rejected)
        {
            return false;
        }

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.Rejected,
            contentType: EnumCoreContentType.Article
        );

        bool wasPublished = Status == EnumContentStatus.Published;

        Status = EnumContentStatus.Rejected;
        RejectionReason = reason;

        AddDomainEvent(
            new CommissionedContentRejectedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Article,
                CustomerId: CustomerId,
                Title: Title,
                Reason: reason
            )
        );

        if (wasPublished)
        {
            AddDomainEvent(new ArticleUnpublishedEvent(ArticleId: Id));
        }

        return true;
    }

    /// <summary>
    /// Archives the article, removing it from all public feeds without deleting it.
    /// Archiving is reversible — Cloudinary images are <b>not</b> deleted.
    /// </summary>
    /// <returns><c>true</c> if archived; <c>false</c> if already archived.</returns>
    public bool Archive()
    {
        if (Status == EnumContentStatus.Archived)
        {
            return false;
        }

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.Archived,
            contentType: EnumCoreContentType.Article
        );

        bool wasPublished = Status == EnumContentStatus.Published;

        Status = EnumContentStatus.Archived;

        if (wasPublished)
        {
            AddDomainEvent(new ArticleUnpublishedEvent(ArticleId: Id));
        }

        return true;
    }

    /// <summary>
    /// Declares the article's removal, capturing the cover file id and the
    /// body-image storage keys before the row disappears so post-commit
    /// consumers (cache invalidation, remote-asset cleanup) can act without
    /// re-querying deleted rows. Called by the delete flow immediately
    /// before the repository removal.
    /// </summary>
    /// <param name="bodyImageStorageKeys">The storage keys of the article's body images.</param>
    public void MarkDeleted(IReadOnlyList<string> bodyImageStorageKeys)
    {
        AddDomainEvent(
            new ArticleDeletedEvent(
                ArticleId: Id,
                CoverFileId: CoverImageFileId,
                BodyImageStorageKeys: bodyImageStorageKeys
            )
        );
    }

    /// <summary>
    /// Flags the article for manual Facebook &amp; Instagram promotion.
    /// Called by the Commerce payment verification flow only.
    /// </summary>
    public void StampSocialBoost() => SocialBoost = true;

    /// <summary>
    /// Activates the article's paid promotion (À-la-Une) placement until the given date.
    /// Called by the Commerce payment verification flow only.
    /// </summary>
    /// <param name="promotionLevelId">
    /// The promotion level purchased, used to determine the homepage grid spot.
    /// </param>
    /// <param name="until">
    /// When the promotion expires (<c>payment.verified_at + promotion_level.duration_days</c>,
    /// the verification instant truncated to whole milliseconds).
    /// </param>
    public void StampPromotion(Guid promotionLevelId, DateTimeOffset until)
    {
        IsPromoted = true;
        PromotionLevelId = promotionLevelId;
        PromotedUntil = until;
    }

    /// <summary>
    /// Force-removes the active paid promotion. SuperAdmin only.
    /// Clears the purchased level alongside the window so no stale placement
    /// data outlives the promotion, and records the audit trail needed for
    /// future pro-rata refund calculation.
    /// </summary>
    /// <param name="unpromotedBy">
    /// Identity of the SuperAdmin performing the force-unpromote, read from JWT claims.
    /// </param>
    /// <param name="reason">
    /// Mandatory reason for the force-unpromote (e.g. "government request", "policy violation").
    /// </param>
    /// <exception cref="ContentRuleException">
    /// Thrown when the article does not have an active promotion.
    /// </exception>
    public void ForceUnpromote(string unpromotedBy, string reason, DateTimeOffset now)
    {
        if (!IsPromoted)
        {
            throw new ContentRuleException(ContentRuleCodes.ArticleNotPromoted);
        }

        IsPromoted = false;
        PromotedUntil = null;
        PromotionLevelId = null;
        UnpromotedAt = now;
        UnpromotedBy = unpromotedBy;
        UnpromotedReason = reason;

        AddDomainEvent(
            new ContentPromotionRemovedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Article,
                CustomerId: CustomerId,
                Title: Title,
                Reason: reason
            )
        );
    }

    /// <summary>
    /// Adds an image row to this article. The identifier is supplied by the caller because the
    /// storage public id is derived from it before the upload happens.
    /// </summary>
    /// <param name="id">The image row's identifier.</param>
    /// <param name="storageKey">The provider storage key of the uploaded asset.</param>
    /// <param name="url">The public delivery URL of the uploaded asset.</param>
    /// <param name="imageType">Whether the image is the cover or a body illustration.</param>
    /// <returns>The image row that was added.</returns>
    public ArticleImageEntity AddImage(Guid id, string storageKey, string url, EnumArticleImageType imageType)
    {
        ArticleImageEntity image = ArticleImageEntity.Create(
            id: id,
            articleId: Id,
            storageKey: storageKey,
            url: url,
            imageType: imageType
        );

        Images.Add(image);

        return image;
    }

    /// <summary>
    /// Removes the cover image row, returning it so the caller can release the stored asset,
    /// or null when the article has no cover.
    /// </summary>
    /// <returns>The removed cover row, or <c>null</c>.</returns>
    public ArticleImageEntity? RemoveCoverImage()
    {
        ArticleImageEntity? cover = Images.FirstOrDefault(image => image.ImageType == EnumArticleImageType.Cover);

        if (cover is not null)
        {
            Images.Remove(cover);
        }

        return cover;
    }

    /// <summary>
    /// Removes the body image rows carrying the given storage keys, returning them so the
    /// caller can release the stored assets.
    /// </summary>
    /// <param name="storageKeys">The storage keys of the rows to remove.</param>
    /// <returns>The removed rows.</returns>
    public IReadOnlyList<ArticleImageEntity> RemoveBodyImages(IEnumerable<string> storageKeys)
    {
        HashSet<string> keys = storageKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<ArticleImageEntity> removed = Images
            .Where(image => image.ImageType == EnumArticleImageType.Body && keys.Contains(image.StorageKey))
            .ToList();

        foreach (ArticleImageEntity image in removed)
        {
            Images.Remove(image);
        }

        return removed;
    }

    /// <summary>
    /// Replaces the tag set with the given ids, raising one <see cref="TagGraphChangedEvent" />
    /// per tag that joins or leaves. An identical set writes nothing and raises nothing.
    /// </summary>
    /// <param name="tagIds">The complete tag set this row should carry.</param>
    /// <returns><c>true</c> if the set changed; otherwise <c>false</c>.</returns>
    public bool ReplaceTags(IReadOnlyCollection<Guid> tagIds)
    {
        HashSet<Guid> desired = tagIds.ToHashSet();
        HashSet<Guid> current = Tags.Select(tag => tag.TagId).ToHashSet();

        if (desired.SetEquals(current))
        {
            return false;
        }

        foreach (ArticleTagEntity removed in Tags.Where(tag => !desired.Contains(tag.TagId)).ToList())
        {
            Tags.Remove(removed);
            AddDomainEvent(new TagGraphChangedEvent(TagId: removed.TagId));
        }

        foreach (Guid tagId in desired.Where(id => !current.Contains(id)))
        {
            Tags.Add(ArticleTagEntity.Create(id: Guid.NewGuid(), articleId: Id, tagId: tagId));
            AddDomainEvent(new TagGraphChangedEvent(TagId: tagId));
        }

        return true;
    }

    /// <summary>
    /// Replaces the credited-artist set with the given ids. An identical set writes nothing.
    /// </summary>
    /// <param name="artistIds">The complete artist set this article should credit.</param>
    /// <returns><c>true</c> if the set changed; otherwise <c>false</c>.</returns>
    public bool ReplaceArtists(IReadOnlyCollection<Guid> artistIds)
    {
        HashSet<Guid> desired = artistIds.ToHashSet();
        HashSet<Guid> current = Artists.Select(credit => credit.ArtistId).ToHashSet();

        if (desired.SetEquals(current))
        {
            return false;
        }

        foreach (ArticleArtistEntity removed in Artists.Where(credit => !desired.Contains(credit.ArtistId)).ToList())
        {
            Artists.Remove(removed);
        }

        foreach (Guid artistId in desired.Where(id => !current.Contains(id)))
        {
            Artists.Add(ArticleArtistEntity.Create(id: Guid.NewGuid(), articleId: Id, artistId: artistId));
        }

        return true;
    }
}
