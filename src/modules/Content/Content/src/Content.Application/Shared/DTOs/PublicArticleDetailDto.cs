namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of a published article. Carries no audit trail, no commercial
/// linkage and no editorial state — those stay on <see cref="ArticleDetailDto" /> for admin.
/// </summary>
public record PublicArticleDetailDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Headline,
    string Body,
    string? CoverImageUrl,
    bool IsPromoted,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    IReadOnlyList<ArticleImageDto> Images,
    IReadOnlyList<TagDto> Tags,
    int ReadTimeInMinutes,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    int BookmarkCount,
    PublicAuthorDto? Author = null,
    bool IsLiked = false,
    bool IsBookmarked = false
);
