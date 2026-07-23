using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision;

/// <summary>
/// Command for a moderator to ask a submitter to revise and resubmit their pending community
/// lyrics submission, rather than rejecting it outright.
/// </summary>
/// <param name="Id">The unique identifier of the submission needing revision.</param>
/// <param name="Note">The mandatory note describing the requested changes.</param>
/// <param name="ReviewerId">The identity user UUID of the moderator making the decision.</param>
public record AdminRequestLyricsSubmissionRevisionCommand(Guid Id, string Note, Guid ReviewerId)
    : ICommand<AdminRequestLyricsSubmissionRevisionResult>;

/// <summary>
/// Result of the <see cref="AdminRequestLyricsSubmissionRevisionCommand" /> indicating whether
/// the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminRequestLyricsSubmissionRevisionResult(bool IsSuccess);
