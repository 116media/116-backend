using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// A community-submitted new song, pending moderation before it becomes a real
/// <see cref="LyricsEntity" />. Distinct from the editorial <c>Draft</c> status — a submission
/// isn't a lyrics record yet, it's a proposal to create one.
/// </summary>
public class LyricsSubmissionEntity : Aggregate<Guid>
{
    /// <summary>
    /// The title of the submitted song.
    /// </summary>
    public string SongTitle { get; private set; } = null!;

    /// <summary>
    /// The name of the performing artist, as entered by the submitter.
    /// </summary>
    public string ArtistName { get; private set; } = null!;

    /// <summary>
    /// The full submitted lyrics text.
    /// </summary>
    public string LyricsText { get; private set; } = null!;

    /// <summary>
    /// ISO 639-1 language code of the submitted lyrics.
    /// </summary>
    public string Language { get; private set; } = null!;

    /// <summary>
    /// The identity user UUID of the user who submitted this song.
    /// No FK to the identity schema by design.
    /// </summary>
    public Guid SubmittedByUserId { get; private set; }

    /// <summary>
    /// Current moderation status of this submission.
    /// </summary>
    public EnumSubmissionStatus Status { get; private set; }

    /// <summary>
    /// The identity user UUID of the moderator who reviewed this submission. <c>null</c> until
    /// reviewed.
    /// </summary>
    public Guid? ReviewedByUserId { get; private set; }

    /// <summary>
    /// The moderator's note explaining a rejection or revision request. <c>null</c> until set.
    /// </summary>
    public string? ReviewNote { get; private set; }

    /// <summary>
    /// The lyrics record created from this submission once approved. <c>null</c> until approved.
    /// </summary>
    public Guid? PublishedLyricsId { get; private set; }

    private LyricsSubmissionEntity() { }

    /// <summary>
    /// Submits a new song for moderation.
    /// </summary>
    /// <param name="id">The unique identifier for this submission.</param>
    /// <param name="songTitle">The song title.</param>
    /// <param name="artistName">The performing artist name.</param>
    /// <param name="lyricsText">The full lyrics text.</param>
    /// <param name="language">ISO 639-1 language code.</param>
    /// <param name="userId">The identity user UUID of the submitter.</param>
    /// <returns>A new <see cref="LyricsSubmissionEntity" /> in <c>Pending</c> status.</returns>
    public static LyricsSubmissionEntity Submit(
        Guid id,
        string songTitle,
        string artistName,
        string lyricsText,
        string language,
        Guid userId
    )
    {
        return new LyricsSubmissionEntity
        {
            Id = id,
            SongTitle = songTitle,
            ArtistName = artistName,
            LyricsText = lyricsText,
            Language = language,
            SubmittedByUserId = userId,
            Status = EnumSubmissionStatus.Pending,
        };
    }

    /// <summary>
    /// Marks this submission approved and links it to the newly created lyrics record.
    /// Called after the lyrics record itself is successfully created — a separate, individually
    /// safe step from the lyrics record's own creation and commit.
    /// </summary>
    /// <param name="reviewedByUserId">The identity user UUID of the reviewing moderator.</param>
    /// <param name="publishedLyricsId">The lyrics record created from this submission.</param>
    public void Approve(Guid reviewedByUserId, Guid publishedLyricsId)
    {
        Status = EnumSubmissionStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        PublishedLyricsId = publishedLyricsId;

        RaiseDecidedEvent();
    }

    /// <summary>
    /// Rejects this submission outright with a mandatory note.
    /// </summary>
    /// <param name="reviewedByUserId">The identity user UUID of the reviewing moderator.</param>
    /// <param name="note">The reason for rejection.</param>
    public void Reject(Guid reviewedByUserId, string note)
    {
        Status = EnumSubmissionStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        ReviewNote = note;

        RaiseDecidedEvent();
    }

    /// <summary>
    /// Asks the submitter to revise and resubmit the content.
    /// </summary>
    /// <param name="reviewedByUserId">The identity user UUID of the reviewing moderator.</param>
    /// <param name="note">The requested changes.</param>
    public void RequestRevision(Guid reviewedByUserId, string note)
    {
        Status = EnumSubmissionStatus.NeedsRevision;
        ReviewedByUserId = reviewedByUserId;
        ReviewNote = note;

        RaiseDecidedEvent();
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
