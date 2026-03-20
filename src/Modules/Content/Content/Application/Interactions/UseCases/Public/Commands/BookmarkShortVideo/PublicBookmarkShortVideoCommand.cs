using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo;

/// <summary>
/// Command to bookmark a short video for the authenticated user.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video to bookmark.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
public record PublicBookmarkShortVideoCommand(Guid ShortVideoId, Guid UserId)
    : ICommand<PublicBookmarkShortVideoResult>;

/// <summary>
/// Result of the <see cref="PublicBookmarkShortVideoCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicBookmarkShortVideoResult(bool IsSuccess);
