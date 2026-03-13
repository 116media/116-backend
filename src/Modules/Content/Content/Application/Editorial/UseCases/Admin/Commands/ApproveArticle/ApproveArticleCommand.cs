using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;

/// <summary>
/// Command for approving an article that is pending editorial review.
/// Transitions the article from <c>PendingReview</c> to <c>Approved</c>.
/// </summary>
/// <param name="Id">The unique identifier of the article to approve.</param>
public record ApproveArticleCommand(string Id) : ICommand;
