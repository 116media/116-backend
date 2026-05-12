using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a lyrics page.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics record.</param>
/// <param name="SongTitle">The title of the song.</param>
/// <param name="ArtistName">The name of the artist.</param>
/// <param name="LyricsText">The full lyrics text.</param>
/// <param name="Language">The ISO-639 language code of the lyrics (e.g., "fr", "en").</param>
/// <param name="VideoId">The linked video identifier, or null if not linked.</param>
/// <param name="MetaTitle">Custom SEO meta title, or null.</param>
/// <param name="MetaDescription">Custom SEO meta description, or null.</param>
/// <param name="MetaKeywords">Custom SEO meta keywords, or null.</param>
/// <param name="AuthorId">The identity user UUID of the author.</param>
/// <param name="Author">The resolved author profile, or null when listing.</param>
public record LyricsDto(
    Guid Id,
    string SongTitle,
    string ArtistName,
    string LyricsText,
    string Language,
    Guid? VideoId,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string AuthorId,
    AuthorDto? Author = null
) : AuditableDto;
