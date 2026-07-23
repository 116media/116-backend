using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics;

/// <summary>
/// Command for submitting a lyrics page for review or payment.
/// Free lyrics pages transition to <c>PendingReview</c>; paid lyrics pages transition to <c>PendingPayment</c>.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics page to submit.</param>
public record AdminSubmitLyricsCommand(string Id) : ICommand<AdminSubmitLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminSubmitLyricsCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminSubmitLyricsResult(bool IsSuccess);
