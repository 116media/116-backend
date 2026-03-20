using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo;

/// <summary>
/// Command to remove a bookmark from a short video.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video to unbookmark.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
public record PublicUnbookmarkShortVideoCommand(Guid ShortVideoId, Guid UserId)
    : ICommand<PublicUnbookmarkShortVideoResult>;

/// <summary>
/// Result of the <see cref="PublicUnbookmarkShortVideoCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicUnbookmarkShortVideoResult(bool IsSuccess);
