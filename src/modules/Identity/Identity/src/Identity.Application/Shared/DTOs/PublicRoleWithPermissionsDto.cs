namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// The public projection of a role grant together with its permissions. Lifecycle state stays
/// on <see cref="RoleWithPermissionsDto" /> for admin.
/// </summary>
public record PublicRoleWithPermissionsDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyCollection<PublicPermissionDto> Permissions
);
