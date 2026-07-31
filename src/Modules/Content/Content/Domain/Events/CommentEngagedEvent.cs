using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised by the comment-like aggregate when a like row is created or
/// removed. Consumed by the comment engagement handler, which applies the
/// comment-local like counter. The counter is comment-local, so the event
/// carries no engagement kind.
/// </summary>
/// <param name="CommentId">The comment the like targets.</param>
/// <param name="Delta"><c>+1</c> for a created like row, <c>-1</c> for a removed one.</param>
public record CommentEngagedEvent(Guid CommentId, int Delta) : IDomainEvent;
