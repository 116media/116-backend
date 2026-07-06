using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticleComment;

/// <summary>
/// Command to remove a user's like from an article comment. Idempotent: unliking a comment
/// the user has not liked is a no-op that still succeeds.
/// </summary>
/// <param name="CommentId">The unique identifier of the comment to unlike.</param>
/// <param name="UserId">The identity user UUID of the user unliking the comment.</param>
public record PublicUnlikeArticleCommentCommand(Guid CommentId, Guid UserId)
    : ICommand<PublicUnlikeArticleCommentResult>;

/// <summary>
/// Result of the <see cref="PublicUnlikeArticleCommentCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicUnlikeArticleCommentResult(bool IsSuccess);
