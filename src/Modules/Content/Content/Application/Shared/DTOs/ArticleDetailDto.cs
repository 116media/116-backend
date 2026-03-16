namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a single article detail view.
/// Extends the summary with full body content, SEO metadata, images, and tags.
/// </summary>
/// <param name="Id">The unique identifier of the article.</param>
/// <param name="CategoryId">The identifier of the article's category.</param>
/// <param name="CategoryName">The display name of the article's category.</param>
/// <param name="Title">The article display title.</param>
/// <param name="Slug">The URL-safe slug used in public article URLs.</param>
/// <param name="Headline">The short teaser text.</param>
/// <param name="Body">The full rich-text HTML body of the article.</param>
/// <param name="CoverImageUrl">The URL of the cover image, or null if not set.</param>
/// <param name="AuthorId">The identity user UUID of the author.</param>
/// <param name="Status">The current editorial workflow status.</param>
/// <param name="RejectionReason">The rejection reason, if the article was rejected.</param>
/// <param name="IsFeatured">Whether the article has an active featured placement.</param>
/// <param name="FeaturedUntil">When the featured placement expires, or null.</param>
/// <param name="PublishedAt">When the article was published, or null if not yet published.</param>
/// <param name="MetaTitle">Custom SEO meta title, or null to fall back to <paramref name="Title"/>.</param>
/// <param name="MetaDescription">Custom SEO meta description, or null.</param>
/// <param name="Images">All image assets associated with this article.</param>
/// <param name="Tags">Tags applied to this article for discovery and SEO.</param>
/// <param name="ReadTimeInMinutes">Estimated reading time in minutes, computed from the body word count.</param>
/// <param name="CreatedAt">When the article was created.</param>
/// <param name="UpdatedAt">When the article was last updated.</param>
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
    string Status,
    string? RejectionReason,
    bool IsFeatured,
    DateTimeOffset? FeaturedUntil,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    IReadOnlyList<ArticleImageDto> Images,
    IReadOnlyList<TagDto> Tags,
    int ReadTimeInMinutes,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
