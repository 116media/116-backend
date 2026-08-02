using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision;

/// <summary>
/// Command for a moderator to decide a pending translation revision's fate directly, bypassing
/// the community vote tally. A single route in either direction, rather than separate accept/
/// reject endpoints, since the spec's own endpoint table defines exactly one moderator-decision
/// route for this workflow.
/// </summary>
/// <param name="Id">The unique identifier of the revision to decide.</param>
/// <param name="Accept">
/// <c>true</c> to accept the revision (applying its proposed text to the translation),
/// <c>false</c> to reject it.
/// </param>
/// <param name="DecidedByUserId">The identity user UUID of the moderator making the decision.</param>
public record AdminDecideTranslationRevisionCommand(Guid Id, bool Accept, Guid DecidedByUserId)
    : ICommand<AdminDecideTranslationRevisionResult>;

/// <summary>
/// Result of the <see cref="AdminDecideTranslationRevisionCommand" /> indicating whether the
/// operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDecideTranslationRevisionResult(bool IsSuccess);
