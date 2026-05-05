using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Command for updating the content and metadata of an existing lyrics page.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics record to update.</param>
/// <param name="SongTitle">The song title.</param>
/// <param name="ArtistName">The performing artist name.</param>
/// <param name="LyricsText">The full lyrics text.</param>
/// <param name="Language">ISO 639-1 language code (e.g., "fr", "en").</param>
/// <param name="VideoId">Optional linked video UUID. Null to unlink.</param>
public record AdminUpdateLyricsCommand(
    string Id,
    string SongTitle,
    string ArtistName,
    string LyricsText,
    string Language,
    Guid? VideoId
) : ICommand<AdminUpdateLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateLyricsCommand" /> containing the updated lyrics details.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record AdminUpdateLyricsResult(LyricsDto Lyrics);
