using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeLyrics;

/// <summary>
/// Command to record that a user has liked a lyrics page.
/// </summary>
/// <param name="LyricsId">The unique identifier of the lyrics page to like.</param>
/// <param name="UserId">The identity user UUID of the user liking the lyrics page.</param>
public record PublicLikeLyricsCommand(Guid LyricsId, Guid UserId) : ICommand<PublicLikeLyricsResult>;

/// <summary>
/// Result of the <see cref="PublicLikeLyricsCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicLikeLyricsResult(bool IsSuccess);
