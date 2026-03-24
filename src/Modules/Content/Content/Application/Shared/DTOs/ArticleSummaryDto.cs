using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an article in list and feed views.
/// Contains all fields needed to render an article card, excluding the rich-text body.
/// </summary>
/// <param name="Id">The unique identifier of the article.</param>
/// <param name="CategoryId">The identifier of the article's category.</param>
/// <param name="CategoryName">The display name of the article's category.</param>
/// <param name="Title">The article display title.</param>
/// <param name="Slug">The URL-safe slug used in public article URLs.</param>
/// <param name="Headline">The short teaser text shown on article cards.</param>
/// <param name="CoverImageUrl">The URL of the cover image, or null if not set.</param>
/// <param name="AuthorId">The identity user UUID of the author.</param>
/// <param name="Status">The current editorial workflow status.</param>
/// <param name="IsFeatured">Whether the article has an active featured placement.</param>
/// <param name="PublishedAt">When the article was published, or null if not yet published.</param>
public record ArticleSummaryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Headline,
    string? CoverImageUrl,
    string AuthorId,
    string Status,
    bool IsFeatured,
    DateTimeOffset? PublishedAt
) : AuditableDto;
