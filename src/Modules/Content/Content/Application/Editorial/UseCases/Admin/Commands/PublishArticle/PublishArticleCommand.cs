using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle;

/// <summary>
/// Command for publishing an approved article, making it live and visible to all visitors.
/// Transitions the article from <c>Approved</c> to <c>Published</c>.
/// </summary>
/// <param name="Id">The unique identifier of the article to publish.</param>
public record PublishArticleCommand(string Id) : ICommand;
