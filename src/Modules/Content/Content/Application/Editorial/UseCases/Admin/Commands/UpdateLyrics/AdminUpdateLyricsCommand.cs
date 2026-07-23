using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Command for updating the content and metadata of an existing lyrics page.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics record to update.</param>
/// <param name="CategoryId">The category this lyrics page belongs to.</param>
/// <param name="SongTitle">The song title.</param>
/// <param name="ArtistName">The performing artist name.</param>
/// <param name="Slug">The URL-safe slug. Must be unique across all lyrics pages.</param>
/// <param name="LyricsText">The full lyrics text.</param>
/// <param name="Language">ISO 639-1 language code (e.g., "fr", "en").</param>
/// <param name="VideoId">Optional linked video UUID. Null to unlink.</param>
/// <param name="CustomerId">Optional B2B customer who commissioned this lyrics page.</param>
/// <param name="OrderItemId">Optional order item this lyrics page fulfils.</param>
public record AdminUpdateLyricsCommand(
    string Id,
    Guid CategoryId,
    string SongTitle,
    string ArtistName,
    string Slug,
    string LyricsText,
    string Language,
    Guid? VideoId,
    Guid? CustomerId,
    Guid? OrderItemId
) : ICommand<AdminUpdateLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateLyricsCommand" /> containing the updated lyrics details.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record AdminUpdateLyricsResult(LyricsDetailDto Lyrics);
