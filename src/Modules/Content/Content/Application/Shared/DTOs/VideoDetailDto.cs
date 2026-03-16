namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a single video detail view.
/// Extends the summary with description, SEO metadata, shooting schedule, and tags.
/// </summary>
/// <param name="Id">The unique identifier of the video.</param>
/// <param name="CategoryId">The identifier of the video's category.</param>
/// <param name="CategoryName">The display name of the video's category.</param>
/// <param name="Title">The video display title.</param>
/// <param name="Slug">The URL-safe slug used in public video URLs.</param>
/// <param name="Description">Optional description shown below the video player.</param>
/// <param name="ThumbnailUrl">The URL of the video thumbnail, or null if not yet uploaded.</param>
/// <param name="ThumbnailStorageKey">The CDN storage key for the thumbnail asset, or null.</param>
/// <param name="AuthorId">The identity user UUID of the author.</param>
/// <param name="Status">The current editorial workflow status.</param>
/// <param name="RejectionReason">The rejection reason, if the video was rejected.</param>
/// <param name="YoutubeVideoId">The YouTube video ID, or null if not yet attached.</param>
/// <param name="IsFeatured">Whether the video has an active featured placement.</param>
/// <param name="FeaturedUntil">When the featured placement expires, or null.</param>
/// <param name="HasLyrics">Whether a lyrics page is linked to this video.</param>
/// <param name="ShootingScheduledAt">The scheduled shooting date, or null.</param>
/// <param name="PublishedAt">When the video was published, or null if not yet published.</param>
/// <param name="MetaTitle">Custom SEO meta title, or null.</param>
/// <param name="MetaDescription">Custom SEO meta description, or null.</param>
/// <param name="Tags">Tags applied to this video for discovery and SEO.</param>
/// <param name="CreatedAt">When the video was created.</param>
/// <param name="UpdatedAt">When the video was last updated.</param>
public record VideoDetailDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string? Description,
    string? ThumbnailUrl,
    string? ThumbnailStorageKey,
    string AuthorId,
    string Status,
    string? RejectionReason,
    string? YoutubeVideoId,
    bool IsFeatured,
    DateTimeOffset? FeaturedUntil,
    bool HasLyrics,
    DateTimeOffset? ShootingScheduledAt,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    IReadOnlyList<TagDto> Tags,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
