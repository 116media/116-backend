using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// A published video together with the authenticated user's interaction metadata.
/// </summary>
/// <param name="Video">The published video.</param>
/// <param name="LastInteractedAt">When the user last performed this interaction.</param>
/// <param name="InteractionCount">The number of matching interactions by this user.</param>
/// <param name="RatedStars">The user's current rating, for rated-video results.</param>
/// <param name="LastShareChannel">The channel used by the user's latest share, when reported.</param>
public record UserVideoActivityDto(
    VideoSummaryDto Video,
    DateTimeOffset LastInteractedAt,
    int InteractionCount,
    short? RatedStars = null,
    EnumShareChannel? LastShareChannel = null
);
