using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages;

/// <summary>
/// Query for retrieving a paginated list of packages with an optional active status filter.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="IsActive">Optional filter to return only active or inactive packages.</param>
public record AdminGetAllPackagesQuery(PaginatedRequest PaginatedRequest, bool? IsActive)
    : IQuery<AdminGetAllPackagesResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllPackagesQuery" /> containing the paginated list of packages.
/// </summary>
/// <param name="Packages">The paginated list of package DTOs.</param>
public record AdminGetAllPackagesResult(PaginatedResult<PackageDto> Packages);
