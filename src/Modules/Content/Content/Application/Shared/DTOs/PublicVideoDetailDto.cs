namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of a published video. Carries no audit trail, no commercial linkage
/// and no editorial state — those stay on <see cref="VideoDetailDto" /> for admin.
/// </summary>
public record PublicVideoDetailDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Description,
    string? ThumbnailUrl,
    string? YoutubeVideoUrl,
    bool IsPromoted,
    bool HasLyrics,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    IReadOnlyList<TagDto> Tags,
    int ShareCount,
    decimal RatingAverage,
    int RatingCount,
    PublicAuthorDto? Author = null,
    bool IsRated = false,
    short? RatedStars = null
);
