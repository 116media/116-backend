using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyrics;

/// <summary>
/// Command for approving a lyrics page that is pending editorial review.
/// Transitions the lyrics page from <c>PendingReview</c> to <c>Approved</c>.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics page to approve.</param>
public record AdminApproveLyricsCommand(string Id) : ICommand<AdminApproveLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminApproveLyricsCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminApproveLyricsResult(bool IsSuccess);
