using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.Contracts;

/// <summary>
/// The navigation shell of a lyrics page.
/// </summary>
/// <param name="VideoSlug">The linked video's slug, when one exists.</param>
/// <param name="ArtistSlug">The linked artist's slug, when one exists.</param>
/// <param name="AlbumTracks">The album's other published tracks.</param>
/// <param name="StreamingLinks">The curated and generated streaming links.</param>
public record LyricsPageLinks(
    string? VideoSlug,
    string? ArtistSlug,
    IReadOnlyList<AlbumTrackDto> AlbumTracks,
    IReadOnlyList<StreamingLinkDto> StreamingLinks
);

/// <summary>
/// Assembles the navigation shell of a lyrics page: linked slugs, sibling album tracks and
/// streaming links.
/// </summary>
public interface IPublicLyricsPageService
{
    /// <summary>
    /// Resolves the page links around the given lyrics.
    /// </summary>
    /// <param name="lyrics">The published lyrics the page is built around.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<LyricsPageLinks> ResolveLinksAsync(LyricsEntity lyrics, CancellationToken cancellationToken);
}
