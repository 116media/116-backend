using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an article comment.
/// When the comment is deleted, the Body is null and IsDeleted is true.
/// </summary>
/// <param name="Id">The unique identifier of the comment.</param>
/// <param name="UserId">The identity user UUID of the commenter.</param>
/// <param name="Body">The comment text. Null if the comment has been deleted.</param>
/// <param name="IsDeleted">Whether this comment has been soft-deleted.</param>
public record ArticleCommentDto(Guid Id, Guid UserId, string? Body, bool IsDeleted) : AuditableDto;
