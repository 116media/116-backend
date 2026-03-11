using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Command for submitting an article for review or payment.
/// Free articles transition to <c>PendingReview</c>; paid articles transition to <c>PendingPayment</c>.
/// </summary>
/// <param name="Id">The unique identifier of the article to submit.</param>
public record AdminSubmitArticleCommand(string Id) : ICommand<AdminSubmitArticleResult>;

/// <summary>
/// Result of the <see cref="AdminSubmitArticleCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminSubmitArticleResult(bool IsSuccess);
