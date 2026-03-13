using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Command for creating a new lyrics page on the platform.
/// A lyrics page can be standalone, linked to a video, or linked to an article.
/// </summary>
/// <param name="SongTitle">The title of the song.</param>
/// <param name="ArtistName">The name of the performing artist.</param>
/// <param name="LyricsText">The full lyrics text of the song.</param>
/// <param name="Language">The ISO 639-1 language code (e.g., "fr", "en", "ln").</param>
/// <param name="VideoId">Optional parent video identifier. Links the lyrics to a lyric video or episode.</param>
/// <param name="ArticleId">Optional parent article identifier. Links the lyrics to a dedicated article.</param>
public record CreateLyricsCommand(
    string SongTitle,
    string ArtistName,
    string LyricsText,
    string Language,
    Guid? VideoId,
    Guid? ArticleId
) : ICommand<CreateLyricsResult>;

/// <summary>
/// Result of the <see cref="CreateLyricsCommand" /> containing the newly created lyrics details.
/// </summary>
/// <param name="Lyrics">The created lyrics information.</param>
public record CreateLyricsResult(LyricsDto Lyrics);
