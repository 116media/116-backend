using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeShortVideo;

/// <summary>
/// Command to record that a user has liked a short video.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video to like.</param>
/// <param name="UserId">The identity user UUID of the user liking the short video.</param>
public record PublicLikeShortVideoCommand(Guid ShortVideoId, Guid UserId) : ICommand<PublicLikeShortVideoResult>;

/// <summary>
/// Result of the <see cref="PublicLikeShortVideoCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicLikeShortVideoResult(bool IsSuccess);
