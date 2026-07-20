using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Command for a moderator to approve a pending community lyrics submission, promoting it into
/// a real, published-workflow-eligible lyrics record.
/// </summary>
/// <param name="Id">The unique identifier of the submission to approve.</param>
/// <param name="Slug">
/// The URL-safe slug to assign to the newly created lyrics record, supplied by the reviewing
/// moderator — a submission itself never carries a slug, since one is only decided once the
/// song is actually promoted.
/// </param>
/// <param name="ReviewerId">The identity user UUID of the moderator making the decision.</param>
public record AdminApproveLyricsSubmissionCommand(Guid Id, string Slug, Guid ReviewerId)
    : ICommand<AdminApproveLyricsSubmissionResult>;

/// <summary>
/// Result of the <see cref="AdminApproveLyricsSubmissionCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
/// <param name="LyricsId">The unique identifier of the newly created lyrics record.</param>
public record AdminApproveLyricsSubmissionResult(bool IsSuccess, Guid LyricsId);
