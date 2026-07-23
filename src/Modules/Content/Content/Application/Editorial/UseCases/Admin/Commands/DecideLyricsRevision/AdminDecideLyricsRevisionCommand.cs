using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;

/// <summary>
/// Command for a moderator to decide a pending lyrics-text correction revision's fate
/// directly, bypassing the community vote tally. A single route in either direction, rather
/// than separate accept/reject endpoints, mirroring the equivalent translation revision
/// decision route.
/// </summary>
/// <param name="Id">The unique identifier of the revision to decide.</param>
/// <param name="Accept">
/// <c>true</c> to accept the revision (applying its proposed text to the lyrics page),
/// <c>false</c> to reject it.
/// </param>
/// <param name="DecidedByUserId">The identity user UUID of the moderator making the decision.</param>
public record AdminDecideLyricsRevisionCommand(Guid Id, bool Accept, Guid DecidedByUserId)
    : ICommand<AdminDecideLyricsRevisionResult>;

/// <summary>
/// Result of the <see cref="AdminDecideLyricsRevisionCommand" /> indicating whether the
/// operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDecideLyricsRevisionResult(bool IsSuccess);
