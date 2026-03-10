using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles;

/// <summary>
/// Query for retrieving the authenticated user's assigned roles with their permissions.
/// </summary>
/// <param name="UserId">The unique identifier of the authenticated user.</param>
/// <remarks>
/// The user ID is extracted from the JWT token at the endpoint level.
/// </remarks>
public record PublicGetOwnRolesQuery(Guid UserId) : IQuery<PublicGetOwnRolesResult>;

/// <summary>
/// Result of the <see cref="PublicGetOwnRolesQuery" /> containing the user's roles with permissions.
/// </summary>
/// <param name="Roles">The list of roles assigned to the user, each with their full permission set.</param>
public record PublicGetOwnRolesResult(IReadOnlyList<RoleWithPermissionsDto> Roles);
