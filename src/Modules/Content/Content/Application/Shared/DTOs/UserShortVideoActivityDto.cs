namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Represents a short video together with the requesting user's activity for one collection
/// (liked, bookmarked, or shared).
/// </summary>
/// <param name="ShortVideo">The active short video.</param>
/// <param name="LastInteractedAt">When the user last performed the collection's interaction.</param>
/// <param name="InteractionCount">How many matching interactions the user performed.</param>
public record UserShortVideoActivityDto(
    ShortVideoDto ShortVideo,
    DateTimeOffset LastInteractedAt,
    int InteractionCount
);
