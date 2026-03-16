using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Command for updating an article. Covers all editable fields.
/// Allowed when the article status is <c>Draft</c>, <c>PendingPayment</c>,
/// <c>PendingReview</c>, or <c>Rejected</c>.
/// </summary>
/// <param name="Id">The unique identifier of the article to update.</param>
/// <param name="CategoryId">The category this article belongs to.</param>
/// <param name="Title">The article title.</param>
/// <param name="Slug">The URL-safe slug. Must be unique across all articles.</param>
/// <param name="Headline">The short teaser text (100–300 characters).</param>
/// <param name="Body">The rich-text HTML body containing only Cloudinary image URLs.</param>
/// <param name="CoverImageUrl">Optional URL of the article's primary cover image.</param>
/// <param name="CustomerId">Optional B2B customer who commissioned this article.</param>
/// <param name="OrderItemId">Optional order item this article fulfils.</param>
/// <param name="SocialBoost">Whether the article is flagged for social media promotion.</param>
/// <param name="IsFeatured">Whether the article has an active featured/À-la-Une placement.</param>
/// <param name="FeaturedUntil">When the featured placement expires. <c>null</c> if not featured.</param>
/// <param name="MetaTitle">Optional SEO meta title (max 70 chars).</param>
/// <param name="MetaDescription">Optional SEO meta description (max 160 chars).</param>
public record AdminUpdateArticleCommand(
    string Id,
    Guid CategoryId,
    string Title,
    string Slug,
    string Headline,
    string Body,
    string? CoverImageUrl,
    Guid? CustomerId,
    Guid? OrderItemId,
    bool SocialBoost,
    bool IsFeatured,
    DateTimeOffset? FeaturedUntil,
    string? MetaTitle,
    string? MetaDescription
) : ICommand<AdminUpdateArticleResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateArticleCommand" /> containing the updated article details.
/// </summary>
/// <param name="Article">The updated article detail information.</param>
public record AdminUpdateArticleResult(ArticleDetailDto Article);
