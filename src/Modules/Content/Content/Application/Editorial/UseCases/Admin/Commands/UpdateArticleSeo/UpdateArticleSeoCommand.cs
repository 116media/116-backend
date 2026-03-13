using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Command for updating an article's SEO metadata.
/// </summary>
/// <param name="Id">The unique identifier of the article to update.</param>
/// <param name="MetaTitle">Custom SEO meta title (max 70 chars). Falls back to article title if null.</param>
/// <param name="MetaDescription">Custom SEO meta description (max 160 chars).</param>
public record UpdateArticleSeoCommand(string Id, string? MetaTitle, string? MetaDescription)
    : ICommand<UpdateArticleSeoResult>;

/// <summary>
/// Result of the <see cref="UpdateArticleSeoCommand" /> containing the updated article details.
/// </summary>
/// <param name="Article">The updated article detail information.</param>
public record UpdateArticleSeoResult(ArticleDetailDto Article);
