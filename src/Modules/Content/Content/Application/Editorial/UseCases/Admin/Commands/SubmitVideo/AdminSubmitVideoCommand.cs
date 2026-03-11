using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Command for submitting a video for review or payment.
/// Free videos transition to <c>PendingReview</c>; paid videos transition to <c>PendingPayment</c>.
/// </summary>
/// <param name="Id">The unique identifier of the video to submit.</param>
public record AdminSubmitVideoCommand(string Id) : ICommand<AdminSubmitVideoResult>;

/// <summary>
/// Result of the <see cref="AdminSubmitVideoCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminSubmitVideoResult(bool IsSuccess);
