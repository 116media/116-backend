using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Command for archiving an article, removing it from all public feeds without deleting it.
/// Archiving is reversible — Cloudinary assets are not deleted.
/// </summary>
/// <param name="Id">The unique identifier of the article to archive.</param>
public record ArchiveArticleCommand(string Id) : ICommand;
