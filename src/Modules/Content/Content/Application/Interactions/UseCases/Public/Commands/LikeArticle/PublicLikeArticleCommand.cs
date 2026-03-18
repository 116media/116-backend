using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticle;

/// <summary>
/// Command to record that a user has liked an article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article to like.</param>
/// <param name="UserId">The identity user UUID of the user liking the article.</param>
public record PublicLikeArticleCommand(Guid ArticleId, Guid UserId) : ICommand<PublicLikeArticleResult>;

/// <summary>
/// Result of the <see cref="PublicLikeArticleCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicLikeArticleResult(bool IsSuccess);
