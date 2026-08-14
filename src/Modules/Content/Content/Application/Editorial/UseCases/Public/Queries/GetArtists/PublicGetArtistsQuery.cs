using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;

/// <summary>
/// Query for the public artist directory: artists with surfaceable content, ordered by
/// accent-folded name, optionally filtered by an initial-letter bucket or a name search.
/// The two filters are mutually exclusive — sending both is a 400, never a silent
/// precedence, so a broken client learns it is broken.
/// </summary>
/// <param name="Page">Pagination parameters for the directory grid.</param>
/// <param name="Letter">Optional initial-letter bucket, <c>A</c>–<c>Z</c> or <c>#</c>.</param>
/// <param name="Search">Optional accent-insensitive name search term, minimum two characters.</param>
public record PublicGetArtistsQuery(PaginatedRequest Page, string? Letter, string? Search)
    : IQuery<PublicGetArtistsResult>;

/// <summary>
/// Result of the <see cref="PublicGetArtistsQuery" /> containing the directory page and the
/// letter rail's enablement data.
/// </summary>
/// <param name="Artists">The paginated directory cards.</param>
/// <param name="AvailableLetters">
/// The distinct initial letters over the same content-filtered set, so the rail never
/// enables a letter that leads to an empty page.
/// </param>
public record PublicGetArtistsResult(PaginatedResult<ArtistSummaryDto> Artists, IReadOnlyList<string> AvailableLetters);
