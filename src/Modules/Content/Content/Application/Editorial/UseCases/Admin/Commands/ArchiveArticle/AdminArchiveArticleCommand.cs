using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Command for archiving an article, removing it from all public feeds without deleting it.
/// Archiving is reversible — Cloudinary assets are not deleted.
/// </summary>
/// <param name="Id">The unique identifier of the article to archive.</param>
public record AdminArchiveArticleCommand(string Id) : ICommand<AdminArchiveArticleResult>;

/// <summary>
/// Result of the <see cref="AdminArchiveArticleCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminArchiveArticleResult(bool IsSuccess);
