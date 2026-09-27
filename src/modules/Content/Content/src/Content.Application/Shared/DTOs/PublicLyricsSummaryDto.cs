namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of a lyrics card in list views. Carries no audit trail, no staff
/// identifiers and no editorial state — those stay on <see cref="LyricsSummaryDto" /> for
/// admin.
/// </summary>
public record PublicLyricsSummaryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string SongTitle,
    string ArtistName,
    string Slug,
    string Language,
    Guid? VideoId,
    string? CoverImageUrl,
    DateTimeOffset? PublishedAt,
    int ViewCount,
    int LikeCount,
    int ShareCount,
    bool IsLiked = false
);
