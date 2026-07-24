using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Command to cast a community vote on a pending translation revision. A revision that reaches
/// the auto-accept threshold's net approvals is accepted and applied to its translation in the
/// same operation as this vote.
/// </summary>
/// <param name="RevisionId">The translation revision being voted on.</param>
/// <param name="Vote">Whether the voter approves or rejects the proposed revision.</param>
/// <param name="Comment">Optional free-text comment justifying the vote.</param>
/// <param name="UserId">The identity user UUID of the voter, from JWT claims.</param>
public record PublicVoteOnTranslationRevisionCommand(Guid RevisionId, EnumVote Vote, string? Comment, Guid UserId)
    : ICommand<PublicVoteOnTranslationRevisionResult>;

/// <summary>
/// Result of the <see cref="PublicVoteOnTranslationRevisionCommand" /> indicating whether the
/// vote was recorded successfully.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicVoteOnTranslationRevisionResult(bool IsSuccess);
