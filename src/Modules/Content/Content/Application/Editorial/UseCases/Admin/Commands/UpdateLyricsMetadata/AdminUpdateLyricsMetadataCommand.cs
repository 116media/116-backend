using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;

/// <summary>
/// Command for updating the song-credit metadata of an existing lyrics page.
/// Kept separate from <see cref="_116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.AdminUpdateLyricsCommand" />
/// (content fields), mirroring the existing <c>AdminUpdateLyricsSeoCommand</c> separate-concern
/// pattern. Each field is independently optional — passing null for one field clears only that
/// field, leaving the others untouched.
/// </summary>
/// <param name="Id">The identifier of the lyrics page to update.</param>
/// <param name="Album">The album name, or null to clear.</param>
/// <param name="ReleaseYear">The release year, or null to clear.</param>
/// <param name="Label">The record label, or null to clear.</param>
/// <param name="Songwriter">The credited songwriter, or null to clear.</param>
/// <param name="Producer">The credited producer, or null to clear.</param>
public record AdminUpdateLyricsMetadataCommand(
    Guid Id,
    string? Album,
    short? ReleaseYear,
    string? Label,
    string? Songwriter,
    string? Producer
) : ICommand<AdminUpdateLyricsMetadataResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateLyricsMetadataCommand" /> containing the updated lyrics details.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record AdminUpdateLyricsMetadataResult(LyricsDetailDto Lyrics);
