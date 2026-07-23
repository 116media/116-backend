using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;

/// <summary>
/// Command to propose a community correction to a published lyrics page's canonical text.
/// Never mutates the lyrics record directly — only an accepted revision's later application
/// does, via the vote threshold or a moderator override. Takes only a lyrics identifier —
/// there is no check anywhere for how that record was created (admin-entered,
/// community-submitted, or verified-artist self-uploaded all go through the identical
/// correction flow; there is no trust exemption based on origin).
/// </summary>
/// <param name="LyricsId">The lyrics page being corrected.</param>
/// <param name="ProposedText">The proposed replacement text.</param>
/// <param name="EditSummary">Optional free-text summary of what changed and why, shown to reviewers.</param>
/// <param name="UserId">The identity user UUID of the user proposing the revision, from JWT claims.</param>
public record PublicProposeLyricsRevisionCommand(Guid LyricsId, string ProposedText, string? EditSummary, Guid UserId)
    : ICommand<PublicProposeLyricsRevisionResult>;

/// <summary>
/// Result of the <see cref="PublicProposeLyricsRevisionCommand" />.
/// </summary>
/// <param name="RevisionId">The unique identifier of the newly proposed revision.</param>
public record PublicProposeLyricsRevisionResult(Guid RevisionId);
