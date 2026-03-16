using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;

/// <summary>
/// Command for approving an article that is pending editorial review.
/// Transitions the article from <c>PendingReview</c> to <c>Approved</c>.
/// </summary>
/// <param name="Id">The unique identifier of the article to approve.</param>
public record AdminApproveArticleCommand(string Id) : ICommand<AdminApproveArticleResult>;

/// <summary>
/// Result of the <see cref="AdminApproveArticleCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminApproveArticleResult(bool IsSuccess);
