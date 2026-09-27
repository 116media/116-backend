using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArtists;

/// <summary>
/// Query for retrieving a paginated list of artist profiles for admin management.
/// Supports optional filtering by search term.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Search">Optional search term matched against artist name and bio.</param>
public record AdminGetAllArtistsQuery(PaginatedRequest PaginatedRequest, string? Search)
    : IQuery<AdminGetAllArtistsResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllArtistsQuery" /> containing a paginated list of artist profiles.
/// </summary>
/// <param name="Artists">The paginated result containing artist profile DTOs.</param>
public record AdminGetAllArtistsResult(PaginatedResult<ArtistDto> Artists);
