using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareLyrics;

/// <summary>
/// Command to record a share event for a lyrics page.
/// </summary>
/// <param name="LyricsId">The unique identifier of the lyrics page being shared.</param>
/// <param name="UserId">The identity user UUID of the requesting user, or null if anonymous.</param>
/// <param name="ShareChannel">The channel the share targeted. Null when unreported.</param>
public record PublicShareLyricsCommand(Guid LyricsId, Guid? UserId, EnumShareChannel? ShareChannel = null)
    : ICommand<PublicShareLyricsResult>;

/// <summary>
/// Result of the <see cref="PublicShareLyricsCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicShareLyricsResult(bool IsSuccess);
