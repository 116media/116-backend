using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;

/// <summary>
/// Command to propose a community correction to a published translation. Never mutates the
/// translation directly — only an accepted revision's later application does, via the vote
/// threshold or a moderator override.
/// </summary>
/// <param name="TranslationId">The translation being corrected.</param>
/// <param name="ProposedText">The proposed replacement text.</param>
/// <param name="EditSummary">Optional free-text summary of what changed and why, shown to reviewers.</param>
/// <param name="UserId">The identity user UUID of the user proposing the revision, from JWT claims.</param>
public record PublicProposeTranslationRevisionCommand(
    Guid TranslationId,
    string ProposedText,
    string? EditSummary,
    Guid UserId
) : ICommand<PublicProposeTranslationRevisionResult>;

/// <summary>
/// Result of the <see cref="PublicProposeTranslationRevisionCommand" />.
/// </summary>
/// <param name="RevisionId">The unique identifier of the newly proposed revision.</param>
public record PublicProposeTranslationRevisionResult(Guid RevisionId);
