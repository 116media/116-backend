using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;

/// <summary>
/// Command for rejecting a video during editorial review.
/// Transitions the video from <c>PendingReview</c> to <c>Rejected</c> with a mandatory reason.
/// </summary>
/// <param name="Id">The unique identifier of the video to reject.</param>
/// <param name="Reason">The reason for rejection, visible to the editorial team.</param>
public record RejectVideoCommand(string Id, string Reason) : ICommand;
