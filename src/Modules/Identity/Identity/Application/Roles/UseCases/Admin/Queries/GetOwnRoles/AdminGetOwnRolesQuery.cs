using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles;

/// <summary>
/// Query for retrieving the authenticated admin's assigned roles with their permissions.
/// </summary>
/// <param name="UserId">The unique identifier of the authenticated admin user.</param>
/// <remarks>
/// The user ID is extracted from the JWT token at the endpoint level.
/// </remarks>
public record AdminGetOwnRolesQuery(Guid UserId) : IQuery<AdminGetOwnRolesResult>;

/// <summary>
/// Result of the <see cref="AdminGetOwnRolesQuery" /> containing the admin's roles with permissions.
/// </summary>
/// <param name="Roles">The list of roles assigned to the admin, each with their full permission set.</param>
public record AdminGetOwnRolesResult(IReadOnlyList<RoleWithPermissionsDto> Roles);
