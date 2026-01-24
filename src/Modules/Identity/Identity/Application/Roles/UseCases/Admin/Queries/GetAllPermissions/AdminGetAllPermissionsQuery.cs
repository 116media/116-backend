using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions;

/// <summary>
/// Query used to retrieve all permissions with pagination and filtering (admin only).
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="Search">Optional search term for fuzzy matching on Resource, Action, and Description.</param>
/// <param name="IsActive">Optional filter by active status.</param>
/// <param name="IsDeleted">Optional filter by deleted status.</param>
public record AdminGetAllPermissionsQuery(
    PaginatedRequest PaginatedRequest,
    string? Search = null,
    bool? IsActive = null,
    bool? IsDeleted = null
) : IQuery<AdminGetAllPermissionsResult>;

/// <summary>
/// The result of executing an <see cref="AdminGetAllPermissionsQuery" />.
/// </summary>
/// <param name="Permissions">Paginated result containing permission DTOs.</param>
public record AdminGetAllPermissionsResult(PaginatedResult<PermissionDto> Permissions);
