using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Identity.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Role and Permission entity mappings.
/// Uses dependency injection instead of global static state.
/// </summary>
public static class RoleMapper
{
    /// <summary>
    /// Registers Role and Permission entity mappings into the provided TypeAdapterConfig.
    /// This method does NOT mutate global state.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RoleEntity, RoleDto>();
        config.NewConfig<PermissionEntity, PermissionDto>();
    }

    /// <summary>
    /// Maps a RoleEntity to a RoleDto.
    /// </summary>
    /// <param name="role">The role entity to map.</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>A RoleDto containing role information.</returns>
    public static RoleDto ToRoleDto(this RoleEntity role, IMapper mapper)
    {
        return mapper.Map<RoleDto>(role);
    }

    /// <summary>
    /// Maps a PermissionEntity to a PermissionDto.
    /// </summary>
    /// <param name="permission">The permission entity to map.</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>A PermissionDto containing permission information.</returns>
    public static PermissionDto ToPermissionDto(this PermissionEntity permission, IMapper mapper)
    {
        return mapper.Map<PermissionDto>(permission);
    }

    /// <summary>
    /// Maps a collection of RolePermissionEntity to a collection of PermissionDto.
    /// </summary>
    /// <param name="rolePermissions">The role permissions to map.</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>A read-only collection of PermissionDto.</returns>
    public static IReadOnlyCollection<PermissionDto> ToPermissionDtos(
        this ICollection<RolePermissionEntity> rolePermissions,
        IMapper mapper
    )
    {
        return rolePermissions.Select(rp => rp.Permission.ToPermissionDto(mapper)).ToList();
    }

    /// <summary>
    /// Maps a RoleEntity with its permissions to a RoleWithPermissionsDto.
    /// </summary>
    /// <param name="role">The role entity with permissions loaded.</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>A RoleWithPermissionsDto containing role and permission information.</returns>
    public static RoleWithPermissionsDto ToRoleWithPermissionsDto(this RoleEntity role, IMapper mapper)
    {
        return new RoleWithPermissionsDto(
            Id: role.Id,
            Name: role.Name,
            Description: role.Description,
            IsActive: role.IsActive,
            IsDeleted: role.IsDeleted,
            DeletedAt: role.DeletedAt,
            Permissions: role.RolePermissions.ToPermissionDtos(mapper)
        );
    }
}
