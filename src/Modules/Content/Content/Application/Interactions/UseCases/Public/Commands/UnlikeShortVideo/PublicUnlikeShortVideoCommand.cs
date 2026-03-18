using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo;

/// <summary>
/// Command to remove the authenticated user's like from a short video.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video to unlike.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
public record PublicUnlikeShortVideoCommand(Guid ShortVideoId, Guid UserId) : ICommand<PublicUnlikeShortVideoResult>;

/// <summary>
/// Result of the <see cref="PublicUnlikeShortVideoCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicUnlikeShortVideoResult(bool IsSuccess);
