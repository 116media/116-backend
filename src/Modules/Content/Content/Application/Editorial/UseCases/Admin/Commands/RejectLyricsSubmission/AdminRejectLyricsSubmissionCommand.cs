using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission;

/// <summary>
/// Command for a moderator to reject a pending community lyrics submission outright.
/// </summary>
/// <param name="Id">The unique identifier of the submission to reject.</param>
/// <param name="Note">The mandatory reason for rejection, visible to the submitter.</param>
/// <param name="ReviewerId">The identity user UUID of the moderator making the decision.</param>
public record AdminRejectLyricsSubmissionCommand(Guid Id, string Note, Guid ReviewerId)
    : ICommand<AdminRejectLyricsSubmissionResult>;

/// <summary>
/// Result of the <see cref="AdminRejectLyricsSubmissionCommand" /> indicating whether the
/// operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminRejectLyricsSubmissionResult(bool IsSuccess);
