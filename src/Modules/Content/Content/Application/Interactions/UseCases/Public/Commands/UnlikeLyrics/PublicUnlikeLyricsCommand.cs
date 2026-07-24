using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeLyrics;

/// <summary>
/// Command to remove the authenticated user's like from a lyrics page.
/// </summary>
/// <param name="LyricsId">The unique identifier of the lyrics page to unlike.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
public record PublicUnlikeLyricsCommand(Guid LyricsId, Guid UserId) : ICommand<PublicUnlikeLyricsResult>;

/// <summary>
/// Result of the <see cref="PublicUnlikeLyricsCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicUnlikeLyricsResult(bool IsSuccess);
