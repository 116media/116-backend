using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle;

/// <summary>
/// Command to record that a user (or anonymous visitor) shared an article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article that was shared.</param>
/// <param name="UserId">The identity user UUID of the sharer. Null for anonymous shares.</param>
public record PublicShareArticleCommand(Guid ArticleId, Guid? UserId) : ICommand<PublicShareArticleResult>;

/// <summary>
/// Result of the <see cref="PublicShareArticleCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicShareArticleResult(bool IsSuccess);
