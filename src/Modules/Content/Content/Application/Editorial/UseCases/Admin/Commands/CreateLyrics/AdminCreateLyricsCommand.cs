using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Command for creating a new lyrics page on the platform.
/// A lyrics page can be standalone or linked to a video.
/// </summary>
/// <param name="CategoryId">The category this lyrics page belongs to.</param>
/// <param name="SongTitle">The title of the song.</param>
/// <param name="ArtistName">The name of the performing artist.</param>
/// <param name="Slug">The URL-safe slug for this lyrics page.</param>
/// <param name="LyricsText">The full lyrics text of the song.</param>
/// <param name="Language">The ISO 639-1 language code (e.g., "fr", "en", "ln").</param>
/// <param name="AuthorId">The identity user UUID read from JWT claims.</param>
/// <param name="VideoId">Optional parent video identifier. Links the lyrics to a lyric video or episode.</param>
/// <param name="CustomerId">The B2B customer who commissioned this lyrics page. Null for free content.</param>
/// <param name="OrderItemId">The order item this lyrics page fulfils. Null for free content.</param>
public record AdminCreateLyricsCommand(
    Guid CategoryId,
    string SongTitle,
    string ArtistName,
    string Slug,
    string LyricsText,
    string Language,
    Guid AuthorId,
    Guid? VideoId,
    Guid? CustomerId,
    Guid? OrderItemId
) : ICommand<AdminCreateLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminCreateLyricsCommand" /> containing the newly created lyrics details.
/// </summary>
/// <param name="Lyrics">The created lyrics information.</param>
public record AdminCreateLyricsResult(LyricsDetailDto Lyrics);
