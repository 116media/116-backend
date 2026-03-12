namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a short video clip.
/// </summary>
/// <param name="Id">The unique identifier of the short video.</param>
/// <param name="Title">The display title of the short video.</param>
/// <param name="Slug">The URL-safe slug used as the short video permalink.</param>
/// <param name="VideoUrl">The publicly accessible URL of the video file.</param>
/// <param name="ThumbnailUrl">The URL of the thumbnail image, or null if not set.</param>
/// <param name="HasFullVideo">Whether this short is a teaser linked to a full-length video.</param>
/// <param name="IsActive">Whether the short video is currently visible to users.</param>
/// <param name="ViewCount">The cached view count.</param>
/// <param name="LikeCount">The cached like count.</param>
/// <param name="ShareCount">The cached share count.</param>
/// <param name="BookmarkCount">The cached bookmark count.</param>
/// <param name="CreatedAt">When the short video was created.</param>
public record ShortVideoDto(
    Guid Id,
    string Title,
    string Slug,
    string VideoUrl,
    string? ThumbnailUrl,
    bool HasFullVideo,
    bool IsActive,
    int ViewCount,
    int LikeCount,
    int ShareCount,
    int BookmarkCount,
    DateTime? CreatedAt
);
