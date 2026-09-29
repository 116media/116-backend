using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// A proposed correction to a published lyrics page's canonical text. Never mutates the lyrics
/// record directly — only <see cref="LyricsEntity.ReplaceLyricsText" /> does, once this revision
/// is accepted. Applies uniformly to every published <see cref="LyricsEntity" /> regardless of
/// how it was created — admin-entered, community-submitted, or verified-artist self-uploaded —
/// there is no trust exemption based on origin.
/// </summary>
public class LyricsRevisionEntity : Aggregate<Guid>
{
    /// <summary>
    /// The lyrics page this revision proposes to correct.
    /// </summary>
    public Guid LyricsId { get; private set; }

    /// <summary>
    /// The proposed replacement text.
    /// </summary>
    public string ProposedText { get; private set; } = null!;

    /// <summary>
    /// Optional free-text summary of what changed and why, shown to reviewers.
    /// </summary>
    public string? EditSummary { get; private set; }

    /// <summary>
    /// The identity user UUID of the user who proposed this revision.
    /// No FK to the identity schema by design.
    /// </summary>
    public Guid ProposedByUserId { get; private set; }

    /// <summary>
    /// Current review status of this revision.
    /// </summary>
    public EnumRevisionStatus Status { get; private set; }

    /// <summary>
    /// The identity user UUID of whoever decided this revision's fate. <c>null</c> when the
    /// revision was auto-accepted by the community vote threshold rather than a moderator.
    /// </summary>
    public Guid? DecidedByUserId { get; private set; }

    private LyricsRevisionEntity() { }

    /// <summary>
    /// Proposes a new correction to a published lyrics page's text.
    /// </summary>
    /// <param name="id">The unique identifier for this revision.</param>
    /// <param name="lyricsId">The lyrics page being corrected.</param>
    /// <param name="proposedText">The proposed replacement text.</param>
    /// <param name="editSummary">Optional summary of the change.</param>
    /// <param name="userId">The identity user UUID proposing the revision.</param>
    /// <returns>A new <see cref="LyricsRevisionEntity" /> in <c>Pending</c> status.</returns>
    public static LyricsRevisionEntity Propose(
        Guid id,
        Guid lyricsId,
        string proposedText,
        string? editSummary,
        Guid userId
    )
    {
        return new LyricsRevisionEntity
        {
            Id = id,
            LyricsId = lyricsId,
            ProposedText = proposedText,
            EditSummary = editSummary,
            ProposedByUserId = userId,
            Status = EnumRevisionStatus.Pending,
        };
    }

    /// <summary>
    /// Accepts this revision, either via the community vote threshold or a moderator override.
    /// Idempotent: an already accepted revision reports false and raises nothing; an already
    /// rejected one cannot be flipped.
    /// </summary>
    /// <param name="decidedByUserId">
    /// The moderator who accepted this revision, or <c>null</c> when auto-accepted by the vote
    /// threshold.
    /// </param>
    /// <returns><c>true</c> if the revision transitioned; otherwise <c>false</c>.</returns>
    public bool Accept(Guid? decidedByUserId)
    {
        if (Status == EnumRevisionStatus.Accepted)
        {
            return false;
        }

        if (Status == EnumRevisionStatus.Rejected)
        {
            throw new ContentRuleException(ContentRuleCodes.RevisionAlreadyDecided);
        }

        Status = EnumRevisionStatus.Accepted;
        DecidedByUserId = decidedByUserId;

        AddDomainEvent(
            new LyricsRevisionDecidedEvent(
                RevisionId: Id,
                LyricsId: LyricsId,
                ProposedByUserId: ProposedByUserId,
                Accepted: true,
                ByModerator: decidedByUserId.HasValue
            )
        );

        return true;
    }

    /// <summary>
    /// Rejects this revision, either via the community vote tally or a moderator override.
    /// Idempotent: an already rejected revision reports false and raises nothing; an already
    /// accepted one cannot be flipped.
    /// </summary>
    /// <param name="decidedByUserId">The moderator who rejected this revision.</param>
    /// <returns><c>true</c> if the revision transitioned; otherwise <c>false</c>.</returns>
    public bool Reject(Guid decidedByUserId)
    {
        if (Status == EnumRevisionStatus.Rejected)
        {
            return false;
        }

        if (Status == EnumRevisionStatus.Accepted)
        {
            throw new ContentRuleException(ContentRuleCodes.RevisionAlreadyDecided);
        }

        Status = EnumRevisionStatus.Rejected;
        DecidedByUserId = decidedByUserId;

        AddDomainEvent(
            new LyricsRevisionDecidedEvent(
                RevisionId: Id,
                LyricsId: LyricsId,
                ProposedByUserId: ProposedByUserId,
                Accepted: false,
                ByModerator: true
            )
        );

        return true;
    }
}
