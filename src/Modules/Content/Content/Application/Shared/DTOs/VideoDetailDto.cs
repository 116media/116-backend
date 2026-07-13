using _116.Content.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a single video detail view.
/// Extends the summary with description, SEO metadata, shooting schedule, and tags.
/// </summary>
/// <param name="Id">
/// The unique identifier of the video.
/// </param>
/// <param name="CategoryId">
/// The identifier of the video's category.
/// </param>
/// <param name="CategoryName">
/// The display name of the video's category.
/// </param>
/// <param name="Title">
/// The video display title.
/// </param>
/// <param name="Slug">
/// The URL-safe slug used in public video URLs.
/// </param>
/// <param name="Description">
/// Description shown below the video player.
/// </param>
/// <param name="ThumbnailUrl">
/// The publicly accessible URL of the thumbnail image, resolved from the associated FileEntity.
/// Null if no thumbnail has been uploaded.
/// </param>
/// <param name="AuthorId">
/// The identity user UUID of the author.
/// </param>
/// <param name="Status">
/// The current editorial workflow status.
/// </param>
/// <param name="RejectionReason">
/// The rejection reason, if the video was rejected.
/// </param>
/// <param name="YoutubeVideoUrl">
/// The full YouTube video URL, or null if not yet attached.
/// </param>
/// <param name="SocialBoost">
/// Whether the video is flagged for social media promotion.
/// </param>
/// <param name="IsPromoted">
/// Whether the video has an active paid promotion.
/// </param>
/// <param name="PromotedUntil">
/// When the paid promotion expires, or null.
/// </param>
/// <param name="PromotionLevelId">
/// The UUID of the applied promotion level, or null if not promoted.
/// </param>
/// <param name="PromotionLevelName">
/// The display name of the applied promotion level, or null if not promoted.
/// </param>
/// <param name="HasLyrics">
/// Whether a lyrics page is linked to this video.
/// </param>
/// <param name="ShootingScheduledAt">
/// The scheduled shooting date, or null.
/// </param>
/// <param name="PublishedAt">
/// When the video was published, or null if not yet published.
/// </param>
/// <param name="MetaTitle">
/// Custom SEO meta title, or null.
/// </param>
/// <param name="MetaDescription">
/// Custom SEO meta description, or null.
/// </param>
/// <param name="Tags">
/// Tags applied to this video for discovery and SEO.
/// </param>
/// <param name="ShareCount">
/// Cached number of shares. Incremented by interaction events.
/// </param>
/// <param name="RatingAverage">
/// Cached average star rating (1–5), computed from all submitted ratings.
/// </param>
/// <param name="RatingCount">
/// Cached total number of ratings received.
/// </param>
/// <param name="CustomerId">
/// The B2B customer UUID this video was commissioned for, or null for free content.
/// </param>
/// <param name="CustomerName">
/// The full name of the commissioning customer, or null for free content.
/// </param>
/// <param name="OrderItemId">
/// The order item UUID this video is linked to, or null for free content.
/// </param>
/// <param name="Author">
/// The resolved author profile with avatar URL, or null if the author could not be found.
/// </param>
/// <param name="IsRated">
/// Whether the requesting user has rated this video. Always false for anonymous requests.
/// </param>
/// <param name="RatedStars">
/// The star value (1–5) the requesting user gave this video, or null when anonymous or not yet rated.
/// Seeds the rating UI so a returning rater sees their prior choice.
/// </param>
public record VideoDetailDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Description,
    string? ThumbnailUrl,
    string AuthorId,
    EnumContentStatus Status,
    string? RejectionReason,
    string? YoutubeVideoUrl,
    bool SocialBoost,
    bool IsPromoted,
    DateTimeOffset? PromotedUntil,
    Guid? PromotionLevelId,
    string? PromotionLevelName,
    bool HasLyrics,
    DateTimeOffset? ShootingScheduledAt,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    IReadOnlyList<TagDto> Tags,
    int ShareCount,
    decimal RatingAverage,
    int RatingCount,
    Guid? CustomerId = null,
    string? CustomerName = null,
    Guid? OrderItemId = null,
    AuthorDto? Author = null,
    bool IsRated = false,
    short? RatedStars = null
) : AuditableDto;
