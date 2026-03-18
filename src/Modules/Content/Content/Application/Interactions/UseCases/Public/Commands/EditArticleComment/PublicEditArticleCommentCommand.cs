using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;

/// <summary>
/// Command to edit the body of an existing article comment.
/// Only the comment owner can edit their own comment.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article the comment belongs to.</param>
/// <param name="CommentId">The unique identifier of the comment to edit.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
/// <param name="Body">The new comment text.</param>
public record PublicEditArticleCommentCommand(Guid ArticleId, Guid CommentId, Guid UserId, string Body)
    : ICommand<PublicEditArticleCommentResult>;

/// <summary>
/// Result of the <see cref="PublicEditArticleCommentCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicEditArticleCommentResult(bool IsSuccess);
