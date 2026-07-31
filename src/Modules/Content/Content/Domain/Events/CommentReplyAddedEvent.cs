using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a reply to a top-level article comment is created. Consumed
/// post-commit to notify the parent comment's author over email and the in-app
/// feed; the engagement counter rides the separate
/// <see cref="ArticleEngagedEvent" /> raised alongside.
/// </summary>
/// <param name="ReplyId">The created reply comment.</param>
/// <param name="ParentCommentId">The top-level comment that was replied to.</param>
/// <param name="ArticleId">The article hosting the conversation.</param>
/// <param name="ReplierUserId">The identity user UUID of the replier.</param>
public record CommentReplyAddedEvent(Guid ReplyId, Guid ParentCommentId, Guid ArticleId, Guid ReplierUserId)
    : IDomainEvent;
