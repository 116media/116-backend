using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle;

/// <summary>
/// Command to record that a user has bookmarked an article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article to bookmark.</param>
/// <param name="UserId">The identity user UUID of the user bookmarking the article.</param>
public record PublicBookmarkArticleCommand(Guid ArticleId, Guid UserId) : ICommand<PublicBookmarkArticleResult>;

/// <summary>
/// Result of the <see cref="PublicBookmarkArticleCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicBookmarkArticleResult(bool IsSuccess);
