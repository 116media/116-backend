using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a short video clip.
/// </summary>
/// <param name="Id">The unique identifier of the short video.</param>
/// <param name="Title">The display title of the short video.</param>
/// <param name="Slug">The URL-safe slug used as the short video permalink.</param>
/// <param name="VideoUrl">
/// The publicly accessible URL of the video file, resolved from the associated FileEntity.
/// </param>
/// <param name="ThumbnailUrl">
/// The URL of the thumbnail image. Resolved from the thumbnail FileEntity if a manual
/// thumbnail was uploaded, otherwise auto-generated from the video file URL.
/// </param>
/// <param name="VideoId">The parent full video UUID, or null for standalone shorts.</param>
/// <param name="VideoSlug">The parent full video's slug for deep-linking, or null for standalone shorts.</param>
/// <param name="HasFullVideo">Whether this short is a teaser linked to a full-length video.</param>
/// <param name="IsActive">Whether the short video is currently visible to users.</param>
/// <param name="ViewCount">The cached view count.</param>
/// <param name="LikeCount">The cached like count.</param>
/// <param name="ShareCount">The cached share count.</param>
/// <param name="BookmarkCount">The cached bookmark count.</param>
/// <param name="AuthorId">The identity user UUID of the author.</param>
/// <param name="Author">The resolved author profile, or null when listing.</param>
/// <param name="IsLiked">
/// Whether the requesting user has liked this short video. Always false for anonymous requests.
/// </param>
/// <param name="IsBookmarked">
/// Whether the requesting user has bookmarked this short video. Always false for anonymous requests.
/// </param>
public record ShortVideoDto(
    Guid Id,
    string Title,
    string Slug,
    string? VideoUrl,
    string? ThumbnailUrl,
    Guid? VideoId,
    string? VideoSlug,
    bool HasFullVideo,
    bool IsActive,
    int ViewCount,
    int LikeCount,
    int ShareCount,
    int BookmarkCount,
    string AuthorId,
    AuthorDto? Author = null,
    bool IsLiked = false,
    bool IsBookmarked = false
) : AuditableDto;
