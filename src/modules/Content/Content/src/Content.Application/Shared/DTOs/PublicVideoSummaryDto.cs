namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of a video card in list and feed views. Carries no audit trail,
/// no staff identifiers, no editorial state and no shooting schedule — those stay on
/// <see cref="VideoSummaryDto" /> for admin.
/// </summary>
public record PublicVideoSummaryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string? ThumbnailUrl,
    string? YoutubeVideoUrl,
    bool IsPromoted,
    bool HasLyrics,
    DateTimeOffset? PublishedAt,
    int ShareCount,
    decimal RatingAverage,
    int RatingCount
);
