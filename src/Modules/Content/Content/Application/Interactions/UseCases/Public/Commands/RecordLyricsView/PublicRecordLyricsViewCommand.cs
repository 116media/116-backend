using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView;

/// <summary>
/// Command to record a view event for a lyrics page, carrying the identity signals used to
/// deduplicate repeat views from the same viewer and the read-time signals used to gate
/// whether the view represents a genuine completed read (spec 05).
/// </summary>
/// <param name="LyricsId">The unique identifier of the lyrics page being viewed.</param>
/// <param name="UserId">The identity user UUID of the viewer, or null if anonymous.</param>
/// <param name="DeviceId">The caller's X-Device-Id header value, or null when absent.</param>
/// <param name="IpAddress">The caller's IP address, or null when unresolvable.</param>
/// <param name="UserAgent">The caller's User-Agent header, or null when absent.</param>
/// <param name="DwellMs">Total foreground dwell time reported by the client, in milliseconds.</param>
/// <param name="ScrollDepthRatio">Maximum scroll coverage reached, from 0.0 to 1.0.</param>
public record PublicRecordLyricsViewCommand(
    Guid LyricsId,
    Guid? UserId,
    string? DeviceId,
    string? IpAddress,
    string? UserAgent,
    int DwellMs,
    double ScrollDepthRatio
) : ICommand<PublicRecordLyricsViewResult>;

/// <summary>
/// Result of the <see cref="PublicRecordLyricsViewCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
/// <param name="IsCounted">
/// Whether this view incremented the displayed count. False when the same identity already
/// had a counted view inside the dedup window, or when the reported dwell time and scroll
/// depth do not satisfy the read-time view-counting rule.
/// </param>
public record PublicRecordLyricsViewResult(bool IsSuccess, bool IsCounted);
