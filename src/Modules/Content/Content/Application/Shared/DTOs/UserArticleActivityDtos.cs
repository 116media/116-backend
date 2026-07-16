using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// A published article together with the time the current user bookmarked it.
/// </summary>
public record UserBookmarkedArticleDto(ArticleSummaryDto Article, DateTimeOffset BookmarkedAt);

/// <summary>
/// A published article together with the current user's latest remaining comment activity.
/// </summary>
public record UserCommentedArticleDto(
    ArticleSummaryDto Article,
    ArticleCommentDto LatestComment,
    int CommentCount,
    DateTimeOffset LastCommentedAt
);

/// <summary>
/// A published article together with the current user's like or share activity.
/// </summary>
public record UserArticleActivityDto(
    ArticleSummaryDto Article,
    DateTimeOffset LastInteractedAt,
    int InteractionCount,
    EnumShareChannel? LastShareChannel = null
);
