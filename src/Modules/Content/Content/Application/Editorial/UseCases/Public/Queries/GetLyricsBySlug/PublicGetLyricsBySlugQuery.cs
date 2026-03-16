using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Query for retrieving a lyrics page by its song title and artist name slug combination.
/// </summary>
/// <param name="SongTitle">The song title used as part of the URL slug.</param>
/// <param name="ArtistName">The artist name used as part of the URL slug.</param>
public record PublicGetLyricsBySlugQuery(string SongTitle, string ArtistName) : IQuery<PublicGetLyricsBySlugResult>;

/// <summary>
/// Result of the <see cref="PublicGetLyricsBySlugQuery" /> containing the matching lyrics page.
/// </summary>
/// <param name="Lyrics">The matched lyrics information.</param>
public record PublicGetLyricsBySlugResult(LyricsDto Lyrics);
