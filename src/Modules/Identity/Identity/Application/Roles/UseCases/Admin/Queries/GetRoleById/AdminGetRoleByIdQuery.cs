using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;

/// <summary>
/// Query to retrieve a role by its unique identifier along with its permissions.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to retrieve.</param>
public record AdminGetRoleByIdQuery(string RoleId) : IQuery<AdminGetRoleByIdResult>;

/// <summary>
/// The result of executing an <see cref="AdminGetRoleByIdQuery" />.
/// </summary>
/// <param name="Role">The role details.</param>
/// <param name="Permissions">The permissions assigned to the role.</param>
public record AdminGetRoleByIdResult(RoleDto Role, IReadOnlyCollection<PermissionDto> Permissions);
