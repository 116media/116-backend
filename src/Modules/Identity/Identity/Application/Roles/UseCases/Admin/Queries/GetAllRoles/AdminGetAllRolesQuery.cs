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
) : IQuery<AdminGetAllRolesResult>;

/// <summary>
/// The result of executing an <see cref="AdminGetAllRolesQuery" />.
/// </summary>
/// <param name="Roles">Paginated result containing role DTOs.</param>
public record AdminGetAllRolesResult(PaginatedResult<RoleDto> Roles);
