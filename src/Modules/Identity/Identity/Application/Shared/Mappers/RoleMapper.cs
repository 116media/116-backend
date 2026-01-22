using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using Mapster;

namespace _116.Identity.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Role and Permission entity mappings.
/// </summary>
public static class RoleMapper
{
    /// <summary>
    /// Configures Mapster mappings for RoleEntity and PermissionEntity.
    /// </summary>
    public static void Configure()
    {
        TypeAdapterConfig<RoleEntity, RoleDto>.NewConfig().Compile();
        TypeAdapterConfig<PermissionEntity, PermissionDto>.NewConfig().Compile();
    }

    /// <summary>
    /// Maps a RoleEntity to a RoleDto.
    /// </summary>
    /// <param name="role">The role entity to map.</param>
    /// <returns>A RoleDto containing role information.</returns>
    public static RoleDto ToRoleDto(this RoleEntity role)
    {
        return role.Adapt<RoleDto>();
    }

    /// <summary>
    /// Maps a PermissionEntity to a PermissionDto.
    /// </summary>
    /// <param name="permission">The permission entity to map.</param>
    /// <returns>A PermissionDto containing permission information.</returns>
    public static PermissionDto ToPermissionDto(this PermissionEntity permission)
    {
        return permission.Adapt<PermissionDto>();
    }

    /// <summary>
    /// Maps a collection of RolePermissionEntity to a collection of PermissionDto.
    /// </summary>
    /// <param name="rolePermissions">The role permissions to map.</param>
    /// <returns>A read-only collection of PermissionDto.</returns>
    public static IReadOnlyCollection<PermissionDto> ToPermissionDtos(
        this ICollection<RolePermissionEntity> rolePermissions
    )
    {
        return rolePermissions.Select(rp => rp.Permission.ToPermissionDto()).ToList();
    }

    /// <summary>
    /// Maps a RoleEntity with its permissions to a RoleWithPermissionsDto.
    /// </summary>
    /// <param name="role">The role entity with permissions loaded.</param>
    /// <returns>A RoleWithPermissionsDto containing role and permission information.</returns>
    public static RoleWithPermissionsDto ToRoleWithPermissionsDto(this RoleEntity role)
    {
        return new RoleWithPermissionsDto(
            Id: role.Id,
            Name: role.Name,
            Description: role.Description,
            IsActive: role.IsActive,
            IsDeleted: role.IsDeleted,
            DeletedAt: role.DeletedAt,
            Permissions: role.RolePermissions.ToPermissionDtos()
        );
    }
}
