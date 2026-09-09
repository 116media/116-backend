namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of published lyrics. Carries no audit trail, no commercial linkage
/// and no editorial state — those stay on <see cref="LyricsDetailDto" /> for admin.
/// </summary>
public record PublicLyricsDetailDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string SongTitle,
    string ArtistName,
    string Slug,
    string LyricsText,
    string Language,
    Guid? VideoId,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    string? CoverImageUrl,
    string? Album,
    short? ReleaseYear,
    string? Label,
    string? Songwriter,
    string? Producer,
    IReadOnlyList<TagDto> Tags,
    int ViewCount,
    int LikeCount,
    int ShareCount,
    PublicAuthorDto? Author = null,
    bool IsLiked = false
);
