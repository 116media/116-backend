using _116.Content.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a single article detail view.
/// Extends the summary with full body content, SEO metadata, images, and tags.
/// </summary>
/// <param name="Id">
/// The unique identifier of the article.
/// </param>
/// <param name="CategoryId">
/// The identifier of the article's category.
/// </param>
/// <param name="CategoryName">
/// The display name of the article's category.
/// </param>
/// <param name="Title">
/// The article display title.
/// </param>
/// <param name="Slug">
/// The URL-safe slug used in public article URLs.
/// </param>
/// <param name="Headline">
/// The short teaser text.
/// </param>
/// <param name="Body">
/// The full rich-text HTML body of the article.
/// </param>
/// <param name="CoverImageUrl">
/// The publicly accessible URL of the cover image, resolved from the associated FileEntity.
/// Null if no cover image has been uploaded.
/// </param>
/// <param name="AuthorId">
/// The identity user UUID of the author.
/// </param>
/// <param name="Status">
/// The current editorial workflow status.
/// </param>
/// <param name="RejectionReason">
/// The rejection reason, if the article was rejected.
/// </param>
/// <param name="SocialBoost">
/// Whether the article is flagged for social media promotion.
/// </param>
/// <param name="IsPromoted">
/// Whether the article has an active paid promotion.
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
/// <param name="PublishedAt">
/// When the article was published, or null if not yet published.
/// </param>
/// <param name="MetaTitle">
/// Custom SEO meta title, or null to fall back to <paramref name="Title"/>.
/// </param>
/// <param name="MetaDescription">
/// Custom SEO meta description, or null.
/// </param>
/// <param name="Images">
/// All image assets associated with this article.
/// </param>
/// <param name="Tags">
/// Tags applied to this article for discovery and SEO.
/// </param>
/// <param name="ReadTimeInMinutes">
/// Estimated reading time in minutes, computed from the body word count.
/// </param>
/// <param name="LikeCount">
/// Cached number of likes. Incremented and decremented by interaction events.
/// </param>
/// <param name="CommentCount">
/// Cached number of comments. Incremented and decremented by interaction events.
/// </param>
/// <param name="ShareCount">
/// Cached number of shares. Incremented by interaction events.
/// </param>
/// <param name="BookmarkCount">
/// Cached number of bookmarks. Incremented and decremented by interaction events.
/// </param>
/// <param name="CustomerId">
/// The B2B customer UUID this article was commissioned for, or null for free content.
/// </param>
/// <param name="CustomerName">
/// The full name of the commissioning customer, or null for free content.
/// </param>
/// <param name="OrderItemId">
/// The order item UUID this article is linked to, or null for free content.
/// </param>
/// <param name="Author">
/// The resolved author profile with avatar URL, or null if the author could not be found.
/// </param>
/// <param name="IsLiked">
/// Whether the current authenticated user has liked this article. False for anonymous
/// requests and for authenticated users who have not liked it.
/// </param>
/// <param name="IsBookmarked">
/// Whether the current authenticated user has bookmarked this article. False for anonymous
/// requests and for authenticated users who have not bookmarked it.
/// </param>
public record ArticleDetailDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Headline,
    string Body,
    string? CoverImageUrl,
    string AuthorId,
    EnumContentStatus Status,
    string? RejectionReason,
    bool SocialBoost,
    bool IsPromoted,
    DateTimeOffset? PromotedUntil,
    Guid? PromotionLevelId,
    string? PromotionLevelName,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    IReadOnlyList<ArticleImageDto> Images,
    IReadOnlyList<TagDto> Tags,
    int ReadTimeInMinutes,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    int BookmarkCount,
    Guid? CustomerId = null,
    string? CustomerName = null,
    Guid? OrderItemId = null,
    AuthorDto? Author = null,
    bool IsLiked = false,
    bool IsBookmarked = false
) : AuditableDto;
