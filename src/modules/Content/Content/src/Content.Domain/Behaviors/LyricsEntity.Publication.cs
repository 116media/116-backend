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
/// Publication behaviour of <see cref="LyricsEntity" />. Its state lives in <c>Entities/LyricsEntity.cs</c>.
/// </summary>
public sealed partial class LyricsEntity
{
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

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.PendingPayment,
            contentType: EnumCoreContentType.Lyrics
        );

        Status = EnumContentStatus.PendingPayment;
        return true;
    }

    /// <summary>
    /// Transitions a free lyrics page from <c>Draft</c> → <c>PendingReview</c>,
    /// or a paid lyrics page from <c>PendingPayment</c> → <c>PendingReview</c> after payment is verified.
    /// Only <c>Draft</c>, <c>PendingPayment</c> and <c>Rejected</c> advance: a page whose
    /// editorial state already moved past review (<c>PendingReview</c>, <c>Approved</c>,
    /// <c>Published</c>, <c>Archived</c>) is left untouched, so a replayed payment effect never
    /// pulls approved or live content back into the review queue and retroactive promotion on an
    /// already-live page stamps <see cref="StampPromotion" /> without un-publishing it.
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
    /// Marks the lyrics page as editorially approved (→ <c>Approved</c>).
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
            contentType: EnumCoreContentType.Lyrics
        );

        Status = EnumContentStatus.Approved;
        return true;
    }

    /// <summary>
    /// Publishes the lyrics page and records the publication timestamp.
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
            contentType: EnumCoreContentType.Lyrics
        );

        Status = EnumContentStatus.Published;
        PublishedAt = now;

        AddDomainEvent(
            new CommissionedContentPublishedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Lyrics,
                CustomerId: CustomerId,
                Title: SongTitle,
                Slug: Slug
            )
        );

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

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.Rejected,
            contentType: EnumCoreContentType.Lyrics
        );

        Status = EnumContentStatus.Rejected;
        RejectionReason = reason;

        AddDomainEvent(
            new CommissionedContentRejectedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Lyrics,
                CustomerId: CustomerId,
                Title: SongTitle,
                Reason: reason
            )
        );

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

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.Archived,
            contentType: EnumCoreContentType.Lyrics
        );

        Status = EnumContentStatus.Archived;
        return true;
    }
}
