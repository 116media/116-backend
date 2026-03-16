using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;

/// <summary>
/// Query for getting all roles assigned to a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
public record AdminGetUserRolesQuery(string UserId) : IQuery<AdminGetUserRolesResult>;

/// <summary>
/// Result of the <see cref="AdminGetUserRolesQuery" /> containing the user's roles.
/// </summary>
/// <param name="Roles">The list of roles assigned to the user.</param>
public record AdminGetUserRolesResult(IReadOnlyCollection<RoleDto> Roles);
