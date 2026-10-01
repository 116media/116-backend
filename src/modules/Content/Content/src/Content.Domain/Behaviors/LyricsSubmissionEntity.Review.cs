using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Review decisions of <see cref="LyricsSubmissionEntity" />. Its state lives in <c>Entities/LyricsSubmissionEntity.cs</c>.
/// </summary>
public partial class LyricsSubmissionEntity
{
    /// <summary>
    /// Marks this submission approved and links it to the newly created lyrics record.
    /// Called after the lyrics record itself is successfully created — a separate, individually
    /// safe step from the lyrics record's own creation and commit.
    /// </summary>
    /// <param name="reviewedByUserId">The identity user UUID of the reviewing moderator.</param>
    /// <param name="publishedLyricsId">The lyrics record created from this submission.</param>
    /// <returns><c>true</c> if the submission transitioned; otherwise <c>false</c>.</returns>
    public bool Approve(Guid reviewedByUserId, Guid publishedLyricsId)
    {
        if (Status == EnumSubmissionStatus.Approved)
        {
            return false;
        }

        EnsurePending();

        Status = EnumSubmissionStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        PublishedLyricsId = publishedLyricsId;

        RaiseDecidedEvent();

        return true;
    }

    /// <summary>
    /// Rejects this submission outright with a mandatory note.
    /// </summary>
    /// <param name="reviewedByUserId">The identity user UUID of the reviewing moderator.</param>
    /// <param name="note">The reason for rejection.</param>
    /// <returns><c>true</c> if the submission transitioned; otherwise <c>false</c>.</returns>
    public bool Reject(Guid reviewedByUserId, string note)
    {
        if (Status == EnumSubmissionStatus.Rejected)
        {
            return false;
        }

        EnsurePending();

        Status = EnumSubmissionStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        ReviewNote = note;

        RaiseDecidedEvent();

        return true;
    }

    /// <summary>
    /// Asks the submitter to revise and resubmit the content.
    /// </summary>
    /// <param name="reviewedByUserId">The identity user UUID of the reviewing moderator.</param>
    /// <param name="note">The requested changes.</param>
    /// <returns><c>true</c> if the submission transitioned; otherwise <c>false</c>.</returns>
    public bool RequestRevision(Guid reviewedByUserId, string note)
    {
        if (Status == EnumSubmissionStatus.NeedsRevision)
        {
            return false;
        }

        EnsurePending();

        Status = EnumSubmissionStatus.NeedsRevision;
        ReviewedByUserId = reviewedByUserId;
        ReviewNote = note;

        RaiseDecidedEvent();

        return true;
    }

    /// <summary>
    /// Guards a decision against a submission that already left <c>Pending</c> for a different
    /// outcome. <c>NeedsRevision</c> has no path back, so every decided state is terminal here.
    /// </summary>
    private void EnsurePending()
    {
        if (Status != EnumSubmissionStatus.Pending)
        {
            throw new ContentRuleException(ContentRuleCodes.SubmissionAlreadyDecided);
        }
    }

    /// <summary>
    /// Raises the decision fact from the state the transition just wrote, so
    /// every decision path carries the same payload shape: the review note for
    /// rejections and revision requests, the published lyrics record for
    /// approvals.
    /// </summary>
    private void RaiseDecidedEvent()
    {
        AddDomainEvent(
            new LyricsSubmissionDecidedEvent(
                SubmissionId: Id,
                SubmittedByUserId: SubmittedByUserId,
                Outcome: Status,
                ReviewNote: ReviewNote,
                PublishedLyricsId: PublishedLyricsId
            )
        );
    }
}
