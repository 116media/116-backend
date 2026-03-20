using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;

/// <summary>
/// Command to soft-delete an article comment. Users can only delete their own comments.
/// </summary>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
/// <param name="ArticleId">The unique identifier of the article the comment belongs to.</param>
/// <param name="CommentId">The unique identifier of the comment to delete.</param>
public record PublicDeleteArticleCommentCommand(Guid UserId, Guid ArticleId, Guid CommentId)
    : ICommand<PublicDeleteArticleCommentResult>;

/// <summary>
/// Result of the <see cref="PublicDeleteArticleCommentCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicDeleteArticleCommentResult(bool IsSuccess);
