using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Command for submitting an article for review or payment.
/// Free articles transition to <c>PendingReview</c>; paid articles transition to <c>PendingPayment</c>.
/// </summary>
/// <param name="Id">The unique identifier of the article to submit.</param>
public record SubmitArticleCommand(string Id) : ICommand;
