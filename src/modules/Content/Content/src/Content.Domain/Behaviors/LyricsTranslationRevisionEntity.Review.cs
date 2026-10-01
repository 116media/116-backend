using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Review decisions of <see cref="LyricsTranslationRevisionEntity" />. Its state lives in <c>Entities/LyricsTranslationRevisionEntity.cs</c>.
/// </summary>
public partial class LyricsTranslationRevisionEntity
{
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
            new TranslationRevisionDecidedEvent(
                RevisionId: Id,
                TranslationId: TranslationId,
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
            new TranslationRevisionDecidedEvent(
                RevisionId: Id,
                TranslationId: TranslationId,
                ProposedByUserId: ProposedByUserId,
                Accepted: false,
                ByModerator: true
            )
        );

        return true;
    }
}
