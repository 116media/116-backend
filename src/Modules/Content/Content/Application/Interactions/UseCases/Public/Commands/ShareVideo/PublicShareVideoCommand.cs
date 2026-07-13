using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo;

/// <summary>
/// Command to record that a user (or anonymous visitor) shared a video.
/// </summary>
/// <param name="VideoId">The unique identifier of the video that was shared.</param>
/// <param name="UserId">The identity user UUID of the sharer. Null for anonymous shares.</param>
/// <param name="Platform">The channel the share targeted (e.g. facebook, x, whatsapp). Null when unreported.</param>
public record PublicShareVideoCommand(Guid VideoId, Guid? UserId, string? Platform = null)
    : ICommand<PublicShareVideoResult>;

/// <summary>
/// Result of the <see cref="PublicShareVideoCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicShareVideoResult(bool IsSuccess);
