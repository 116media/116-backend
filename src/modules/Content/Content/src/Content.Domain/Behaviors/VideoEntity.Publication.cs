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
/// Publication behaviour of <see cref="VideoEntity" />. Its state lives in <c>Entities/VideoEntity.cs</c>.
/// </summary>
public sealed partial class VideoEntity
{
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

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.PendingPayment,
            contentType: EnumCoreContentType.Video
        );

        Status = EnumContentStatus.PendingPayment;

        return true;
    }

    /// <summary>
    /// Transitions a free video from <c>Draft</c> → <c>PendingReview</c>,
    /// or a paid video from <c>PendingPayment</c> → <c>PendingReview</c> after payment is verified.
    /// Only <c>Draft</c>, <c>PendingPayment</c> and <c>Rejected</c> advance: a video whose
    /// editorial state already moved past review (<c>PendingReview</c>, <c>Approved</c>,
    /// <c>Published</c>, <c>Archived</c>) is left untouched, so a replayed payment effect never
    /// pulls approved or live content back into the review queue and retroactive promotion on an
    /// already-live video stamps <see cref="StampPromotion" /> without un-publishing it.
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
    /// Marks the video as editorially approved (→ <c>Approved</c>).
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
            contentType: EnumCoreContentType.Video
        );

        Status = EnumContentStatus.Approved;
        return true;
    }

    /// <summary>
    /// Publishes the video. Throws if no YouTube URL has been attached —
    /// enforcing the YouTube gate at the domain level.
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
            contentType: EnumCoreContentType.Video
        );

        if (string.IsNullOrWhiteSpace(YoutubeVideoUrl))
        {
            throw new ContentRuleException(ContentRuleCodes.PublicationRequiresYoutubeUrl);
        }

        Status = EnumContentStatus.Published;
        PublishedAt = now;

        AddDomainEvent(
            new CommissionedContentPublishedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Video,
                CustomerId: CustomerId,
                Title: Title,
                Slug: Slug
            )
        );
        AddDomainEvent(new VideoPublishedEvent(VideoId: Id));

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

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.Rejected,
            contentType: EnumCoreContentType.Video
        );

        bool wasPublished = Status == EnumContentStatus.Published;

        Status = EnumContentStatus.Rejected;
        RejectionReason = reason;

        AddDomainEvent(
            new CommissionedContentRejectedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Video,
                CustomerId: CustomerId,
                Title: Title,
                Reason: reason
            )
        );

        if (wasPublished)
        {
            AddDomainEvent(new VideoUnpublishedEvent(VideoId: Id));
        }

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

        ContentPublicationState.EnsureCanMove(
            from: Status,
            to: EnumContentStatus.Archived,
            contentType: EnumCoreContentType.Video
        );

        bool wasPublished = Status == EnumContentStatus.Published;

        Status = EnumContentStatus.Archived;

        if (wasPublished)
        {
            AddDomainEvent(new VideoUnpublishedEvent(VideoId: Id));
        }

        return true;
    }

    /// <summary>
    /// Declares the video's removal, capturing the thumbnail file id before
    /// the row disappears so post-commit consumers (cache invalidation,
    /// remote-asset cleanup) can act without re-querying a deleted row.
    /// Called by the delete flow immediately before the repository removal.
    /// </summary>
    public void MarkDeleted()
    {
        AddDomainEvent(new VideoDeletedEvent(VideoId: Id, ThumbnailFileId: ThumbnailFileId));
    }
}
