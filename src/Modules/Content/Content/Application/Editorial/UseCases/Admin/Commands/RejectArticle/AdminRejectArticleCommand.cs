using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;

/// <summary>
/// Command for rejecting an article during editorial review.
/// Transitions the article from <c>PendingReview</c> to <c>Rejected</c> with a mandatory reason.
/// </summary>
/// <param name="Id">The unique identifier of the article to reject.</param>
/// <param name="Reason">The reason for rejection, visible to the editorial team.</param>
public record AdminRejectArticleCommand(string Id, string Reason) : ICommand<AdminRejectArticleResult>;

/// <summary>
/// Result of the <see cref="AdminRejectArticleCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminRejectArticleResult(bool IsSuccess);
