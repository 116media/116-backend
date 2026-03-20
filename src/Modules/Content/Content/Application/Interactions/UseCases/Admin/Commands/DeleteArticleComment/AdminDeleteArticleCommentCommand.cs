using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment;

/// <summary>
/// Command to soft-delete any article comment as an admin.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article the comment belongs to.</param>
/// <param name="CommentId">The unique identifier of the comment to delete.</param>
public record AdminDeleteArticleCommentCommand(Guid ArticleId, Guid CommentId)
    : ICommand<AdminDeleteArticleCommentResult>;

/// <summary>
/// Result of the <see cref="AdminDeleteArticleCommentCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteArticleCommentResult(bool IsSuccess);
