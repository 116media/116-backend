using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle;

/// <summary>
/// Command to remove a user's like from an article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article to unlike.</param>
/// <param name="UserId">The identity user UUID of the user removing the like.</param>
public record PublicUnlikeArticleCommand(Guid ArticleId, Guid UserId) : ICommand<PublicUnlikeArticleResult>;

/// <summary>
/// Result of the <see cref="PublicUnlikeArticleCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicUnlikeArticleResult(bool IsSuccess);
