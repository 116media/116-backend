using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;

/// <summary>
/// Command for approving a video that is pending editorial review.
/// Transitions the video from <c>PendingReview</c> to <c>Approved</c>.
/// </summary>
/// <param name="Id">The unique identifier of the video to approve.</param>
public record AdminApproveVideoCommand(string Id) : ICommand<AdminApproveVideoResult>;

/// <summary>
/// Result of the <see cref="AdminApproveVideoCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminApproveVideoResult(bool IsSuccess);
