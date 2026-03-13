using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;

/// <summary>
/// Command for approving a video that is pending editorial review.
/// Transitions the video from <c>PendingReview</c> to <c>Approved</c>.
/// </summary>
/// <param name="Id">The unique identifier of the video to approve.</param>
public record ApproveVideoCommand(string Id) : ICommand;
