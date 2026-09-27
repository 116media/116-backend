namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of an article comment. Keeps the posting time as the display
/// timestamp — named <see cref="PostedAt" /> so the audit convention does not re-attach it —
/// and carries no audit trail.
/// </summary>
public record PublicArticleCommentDto(
    Guid Id,
    Guid UserId,
    string? Body,
    bool IsDeleted,
    DateTime? PostedAt,
    PublicAuthorDto? Author = null,
    Guid? ParentCommentId = null,
    int ReplyCount = 0,
    IReadOnlyList<PublicArticleCommentDto>? Replies = null,
    int LikeCount = 0,
    bool IsLiked = false
);
