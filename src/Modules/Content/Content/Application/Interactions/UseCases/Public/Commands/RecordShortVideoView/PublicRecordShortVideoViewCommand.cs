using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;

/// <summary>
/// Command to record a view event for a short video, carrying the identity signals used
/// to deduplicate repeat views from the same viewer.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video being viewed.</param>
/// <param name="UserId">The identity user UUID of the viewer, or null if anonymous.</param>
/// <param name="DeviceId">The caller's X-Device-Id header value, or null when absent.</param>
/// <param name="IpAddress">The caller's IP address, or null when unresolvable.</param>
/// <param name="UserAgent">The caller's User-Agent header, or null when absent.</param>
public record PublicRecordShortVideoViewCommand(
    Guid ShortVideoId,
    Guid? UserId = null,
    string? DeviceId = null,
    string? IpAddress = null,
    string? UserAgent = null
) : ICommand<PublicRecordShortVideoViewResult>;

/// <summary>
/// Result of the <see cref="PublicRecordShortVideoViewCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
/// <param name="IsCounted">
/// Whether this view incremented the displayed count. False when the same identity already
/// had a counted view inside the dedup window.
/// </param>
public record PublicRecordShortVideoViewResult(bool IsSuccess, bool IsCounted);
