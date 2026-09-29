namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// The public projection of a role grant: name and description only. Lifecycle state stays on
/// <see cref="RoleDto" /> for admin.
/// </summary>
/// <param name="Id">The role identifier.</param>
/// <param name="Name">The role name.</param>
/// <param name="Description">The human-readable description.</param>
public record PublicRoleDto(Guid Id, string Name, string Description);
