using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticleComment;

/// <summary>
/// Command to record that a user has liked an article comment. Idempotent: liking an
/// already-liked comment is a no-op that still succeeds.
/// </summary>
/// <param name="CommentId">The unique identifier of the comment to like.</param>
/// <param name="UserId">The identity user UUID of the user liking the comment.</param>
public record PublicLikeArticleCommentCommand(Guid CommentId, Guid UserId) : ICommand<PublicLikeArticleCommentResult>;

/// <summary>
/// Result of the <see cref="PublicLikeArticleCommentCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicLikeArticleCommentResult(bool IsSuccess);
