using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareShortVideo;

/// <summary>
/// Command to record a share event for a short video.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video being shared.</param>
/// <param name="UserId">The identity user UUID of the requesting user, or null if anonymous.</param>
public record PublicShareShortVideoCommand(Guid ShortVideoId, Guid? UserId) : ICommand<PublicShareShortVideoResult>;

/// <summary>
/// Result of the <see cref="PublicShareShortVideoCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicShareShortVideoResult(bool IsSuccess);
