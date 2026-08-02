using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;

/// <summary>
/// Command for rejecting a lyrics page during editorial review.
/// Transitions the lyrics page from <c>PendingReview</c> to <c>Rejected</c> with a mandatory reason.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics page to reject.</param>
/// <param name="Reason">The reason for rejection, visible to the editorial team.</param>
public record AdminRejectLyricsCommand(string Id, string Reason) : ICommand<AdminRejectLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminRejectLyricsCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminRejectLyricsResult(bool IsSuccess);
