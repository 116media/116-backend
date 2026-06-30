using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an article comment.
/// When the comment is deleted, the Body is null, IsDeleted is true, and Author is null.
/// </summary>
/// <param name="Id">
/// The unique identifier of the comment.
/// </param>
/// <param name="UserId">
/// The identity user UUID of the commenter.
/// </param>
/// <param name="Body">
/// The comment text. Null if the comment has been deleted.
/// </param>
/// <param name="IsDeleted">
/// Whether this comment has been soft-deleted.
/// </param>
/// <param name="Author">
/// The resolved commenter profile with avatar URL, or null when the comment is deleted
/// or the commenter could not be resolved. The email is never populated on the public
/// endpoint.
/// </param>
/// <param name="ParentCommentId">
/// The parent comment ID if this is a reply, or null for a top-level comment.
/// </param>
/// <param name="ReplyCount">
/// The number of non-deleted direct replies to this comment.
/// </param>
/// <param name="Replies">
/// A bounded set of embedded replies, or null when replies are fetched lazily.
/// </param>
/// <param name="LikeCount">
/// The cached number of likes on this comment.
/// </param>
/// <param name="IsLiked">
/// Whether the current viewer has liked this comment. False for anonymous viewers.
/// </param>
public record ArticleCommentDto(
    Guid Id,
    Guid UserId,
    string? Body,
    bool IsDeleted,
    AuthorDto? Author = null,
    Guid? ParentCommentId = null,
    int ReplyCount = 0,
    IReadOnlyList<ArticleCommentDto>? Replies = null,
    int LikeCount = 0,
    bool IsLiked = false
) : AuditableDto;
