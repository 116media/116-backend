using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Shared.DTOs;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;

/// <summary>
/// Command for creating a new role.
/// </summary>
/// <param name="Name">The name of the role (must be unique).</param>
/// <param name="Description">A description of the role's purpose.</param>
public record AdminCreateRoleCommand(string Name, string Description) : ICommand<AdminCreateRoleResult>;

/// <summary>
/// Result of the <see cref="AdminCreateRoleCommand" /> containing the created role details.
/// </summary>
/// <param name="Role">The created role information.</param>
public record AdminCreateRoleResult(RoleDto Role);
