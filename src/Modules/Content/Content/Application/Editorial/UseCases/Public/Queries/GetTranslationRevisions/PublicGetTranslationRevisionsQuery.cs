using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions;

/// <summary>
/// Query for retrieving a translation's full revision history — pending, accepted, and
/// rejected revisions alike.
/// </summary>
/// <param name="TranslationId">The translation whose revision history is being listed.</param>
public record PublicGetTranslationRevisionsQuery(Guid TranslationId) : IQuery<PublicGetTranslationRevisionsResult>;

/// <summary>
/// A single proposed revision in a translation's review history.
/// </summary>
/// <param name="Id">The unique identifier of the revision.</param>
/// <param name="ProposedText">The proposed replacement text.</param>
/// <param name="EditSummary">Optional free-text summary of what changed and why.</param>
/// <param name="ProposedByUserId">The identity user UUID of the user who proposed this revision.</param>
/// <param name="Status">The revision's current review status.</param>
/// <param name="DecidedByUserId">
/// The identity user UUID of whoever decided this revision's fate, or null when auto-accepted
/// by the community vote threshold rather than a moderator.
/// </param>
public record TranslationRevisionDto(
    Guid Id,
    string ProposedText,
    string? EditSummary,
    Guid ProposedByUserId,
    string Status,
    Guid? DecidedByUserId
);

/// <summary>
/// Result of the <see cref="PublicGetTranslationRevisionsQuery" />.
/// </summary>
/// <param name="Revisions">The translation's full revision history, newest first.</param>
public record PublicGetTranslationRevisionsResult(IReadOnlyList<TranslationRevisionDto> Revisions);
