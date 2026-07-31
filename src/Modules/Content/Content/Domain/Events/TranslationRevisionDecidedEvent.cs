using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a translation correction revision is decided — accepted via the
/// community vote threshold or a moderator override, or rejected by a
/// moderator. The acceptance's text application stays in the deciding
/// transaction; only this decision fact fans out, to notify the proposer.
/// </summary>
/// <param name="RevisionId">The decided revision.</param>
/// <param name="TranslationId">The translation the revision proposed to correct.</param>
/// <param name="ProposedByUserId">The identity user UUID of the revision's proposer.</param>
/// <param name="Accepted"><c>true</c> when the revision was accepted, <c>false</c> when rejected.</param>
/// <param name="ByModerator"><c>true</c> for a moderator decision, <c>false</c> for the community vote threshold.</param>
public record TranslationRevisionDecidedEvent(
    Guid RevisionId,
    Guid TranslationId,
    Guid ProposedByUserId,
    bool Accepted,
    bool ByModerator
) : IDomainEvent;
