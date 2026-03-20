using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;

/// <summary>
/// Command to submit or update a star rating for a video.
/// </summary>
/// <param name="VideoId">The unique identifier of the video to rate.</param>
/// <param name="UserId">The identity user UUID of the rater.</param>
/// <param name="Stars">The star rating value (1–5).</param>
public record PublicRateVideoCommand(Guid VideoId, Guid UserId, short Stars) : ICommand<PublicRateVideoResult>;

/// <summary>
/// Result of the <see cref="PublicRateVideoCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicRateVideoResult(bool IsSuccess);
