using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Publication behaviour of <see cref="ArticleEntity" />. Its state lives in <c>Entities/ArticleEntity.cs</c>.
/// </summary>
public sealed partial class ArticleEntity
{
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
}
