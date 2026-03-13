using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;

/// <summary>
/// Command for permanently deleting a draft or rejected article.
/// Also deletes all associated Cloudinary image assets before removing the database record.
/// Only articles in <c>Draft</c> or <c>Rejected</c> status can be deleted.
/// </summary>
/// <param name="Id">The unique identifier of the article to delete.</param>
public record DeleteArticleCommand(string Id) : ICommand;
