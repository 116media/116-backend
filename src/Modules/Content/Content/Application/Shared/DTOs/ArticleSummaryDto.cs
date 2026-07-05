using _116.Content.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an article in list and feed views.
/// Contains all fields needed to render an article card, excluding the rich-text body.
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
/// The short teaser text shown on article cards.
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
/// <param name="IsPromoted">
/// Whether the article has an active paid promotion.
/// </param>
/// <param name="PublishedAt">
/// When the article was published, or null if not yet published.
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
/// <param name="IsLiked">
/// Whether the current authenticated user has liked this article. False for anonymous
/// requests and for authenticated users who have not liked it.
/// </param>
/// <param name="IsBookmarked">
/// Whether the current authenticated user has bookmarked this article. False for anonymous
/// requests and for authenticated users who have not bookmarked it.
/// </param>
public record ArticleSummaryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Headline,
    string? CoverImageUrl,
    string AuthorId,
    EnumContentStatus Status,
    bool IsPromoted,
    DateTimeOffset? PublishedAt,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    int BookmarkCount,
    bool IsLiked = false,
    bool IsBookmarked = false
) : AuditableDto;
