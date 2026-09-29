namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of a short video. Carries no audit trail, no staff identifiers and
/// no lifecycle state — those stay on <see cref="ShortVideoDto" /> for admin.
/// </summary>
public record PublicShortVideoDto(
    Guid Id,
    string Title,
    string Slug,
    string? VideoUrl,
    string? ThumbnailUrl,
    Guid? VideoId,
    string? VideoSlug,
    bool HasFullVideo,
    int ViewCount,
    int LikeCount,
    int ShareCount,
    int BookmarkCount,
    PublicAuthorDto? Author = null,
    bool IsLiked = false,
    bool IsBookmarked = false
);
