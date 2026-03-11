using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;

/// <summary>
/// Command for publishing an approved video.
/// Transitions the video from <c>Approved</c> to <c>Published</c>.
/// Requires a YouTube video ID to be attached before publishing.
/// </summary>
/// <param name="Id">The unique identifier of the video to publish.</param>
public record AdminPublishVideoCommand(string Id) : ICommand<AdminPublishVideoResult>;

/// <summary>
/// Result of the <see cref="AdminPublishVideoCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminPublishVideoResult(bool IsSuccess);
