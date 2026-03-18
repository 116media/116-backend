using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkArticle;

/// <summary>
/// Command to remove a user's bookmark from an article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article to un-bookmark.</param>
/// <param name="UserId">The identity user UUID of the user removing the bookmark.</param>
public record PublicUnbookmarkArticleCommand(Guid ArticleId, Guid UserId) : ICommand<PublicUnbookmarkArticleResult>;

/// <summary>
/// Result of the <see cref="PublicUnbookmarkArticleCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicUnbookmarkArticleResult(bool IsSuccess);
