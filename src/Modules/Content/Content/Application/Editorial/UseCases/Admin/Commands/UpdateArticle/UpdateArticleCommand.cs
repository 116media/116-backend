using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Command for updating an article's content (step 2 of the two-step article creation flow).
/// Also used when revising a <c>Rejected</c> article before resubmission.
/// </summary>
/// <param name="Id">The unique identifier of the article to update.</param>
/// <param name="Headline">The short teaser text (100–300 characters).</param>
/// <param name="Body">The rich-text HTML body containing only Cloudinary image URLs.</param>
/// <param name="CoverImageUrl">Optional URL of the article's primary cover image.</param>
public record UpdateArticleCommand(string Id, string Headline, string Body, string? CoverImageUrl)
    : ICommand<UpdateArticleResult>;

/// <summary>
/// Result of the <see cref="UpdateArticleCommand" /> containing the updated article details.
/// </summary>
/// <param name="Article">The updated article detail information.</param>
public record UpdateArticleResult(ArticleDetailDto Article);
