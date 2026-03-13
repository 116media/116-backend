using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Command for submitting a video for review or payment.
/// Free videos transition to <c>PendingReview</c>; paid videos transition to <c>PendingPayment</c>.
/// </summary>
/// <param name="Id">The unique identifier of the video to submit.</param>
public record SubmitVideoCommand(string Id) : ICommand;
