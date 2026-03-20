using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;

/// <summary>
/// Command to record a view event for a short video.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video being viewed.</param>
public record PublicRecordShortVideoViewCommand(Guid ShortVideoId) : ICommand<PublicRecordShortVideoViewResult>;

/// <summary>
/// Result of the <see cref="PublicRecordShortVideoViewCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicRecordShortVideoViewResult(bool IsSuccess);
