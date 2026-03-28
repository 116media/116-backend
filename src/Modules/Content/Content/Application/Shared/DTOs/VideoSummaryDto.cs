using _116.Content.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a video in list and feed views.
/// Contains all fields needed to render a video card.
/// </summary>
/// <param name="Id">The unique identifier of the video.</param>
/// <param name="CategoryId">The identifier of the video's category.</param>
/// <param name="CategoryName">The display name of the video's category.</param>
/// <param name="Title">The video display title.</param>
/// <param name="Slug">The URL-safe slug used in public video URLs.</param>
/// <param name="ThumbnailUrl">The URL of the video thumbnail, or null if not yet uploaded.</param>
/// <param name="AuthorId">The identity user UUID of the author.</param>
/// <param name="Status">The current editorial workflow status.</param>
/// <param name="YoutubeVideoId">The YouTube video ID, or null if not yet attached.</param>
/// <param name="IsFeatured">Whether the video has an active featured placement.</param>
/// <param name="HasLyrics">Whether a lyrics page is linked to this video.</param>
/// <param name="PublishedAt">When the video was published, or null if not yet published.</param>
public record VideoSummaryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string? ThumbnailUrl,
    string AuthorId,
    EnumContentStatus Status,
    string? YoutubeVideoId,
    bool IsFeatured,
    bool HasLyrics,
    DateTimeOffset? PublishedAt
) : AuditableDto;
