namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of an article card in list and feed views. Carries no audit trail,
/// no staff identifiers and no editorial state — those stay on
/// <see cref="ArticleSummaryDto" /> for admin.
/// </summary>
public record PublicArticleSummaryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Headline,
    string? CoverImageUrl,
    bool IsPromoted,
    DateTimeOffset? PublishedAt,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    int BookmarkCount,
    bool IsLiked = false,
    bool IsBookmarked = false
);
