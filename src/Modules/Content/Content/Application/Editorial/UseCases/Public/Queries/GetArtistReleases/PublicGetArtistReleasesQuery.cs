using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases;

/// <summary>
/// Query for retrieving a page of an artist's releases of a given type, addressed by the
/// artist's slug. One query with a type filter rather than separate album and mixtape
/// queries — the two sections differ by a filter value and a heading, nothing else.
/// </summary>
/// <param name="Slug">The URL-safe slug of the artist profile.</param>
/// <param name="ReleaseType">The release type to filter to.</param>
/// <param name="Page">Pagination parameters for the release list.</param>
public record PublicGetArtistReleasesQuery(string Slug, EnumReleaseType ReleaseType, PaginatedRequest Page)
    : IQuery<PublicGetArtistReleasesResult>;

/// <summary>
/// Result of the <see cref="PublicGetArtistReleasesQuery" /> containing the paginated releases.
/// </summary>
/// <param name="Releases">The artist's paginated releases of the requested type.</param>
public record PublicGetArtistReleasesResult(PaginatedResult<AlbumDto> Releases);
