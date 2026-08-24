using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles;

/// <summary>
/// Query used to retrieve all roles with pagination and filtering (admin only).
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="Search">Optional search term for fuzzy matching on Name and Description.</param>
/// <param name="IsActive">Optional filter by active status.</param>
/// <param name="IsDeleted">Optional filter by deleted status.</param>
public record AdminGetAllRolesQuery(
    PaginatedRequest PaginatedRequest,
    string? Search = null,
    bool? IsActive = null,
    bool? IsDeleted = null
) : IQuery<AdminGetAllRolesResult>, IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Free-text search produces an unbounded key space, so those results are never stored.
    /// </remarks>
    public bool IsCacheable => string.IsNullOrWhiteSpace(Search);

    /// <inheritdoc />
    public string CacheKey =>
        $"lookup:roles:admin:{PaginatedRequest.PageIndex}:{PaginatedRequest.PageSize}"
        + $":{IsActive?.ToString() ?? "any"}:{IsDeleted?.ToString() ?? "any"}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [IdentityCacheTags.Lookups];
}

/// <summary>
/// The result of executing an <see cref="AdminGetAllRolesQuery" />.
/// </summary>
/// <param name="Roles">Paginated result containing role DTOs.</param>
public record AdminGetAllRolesResult(PaginatedResult<RoleDto> Roles);
