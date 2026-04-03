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
        config
            .NewConfig<RoleEntity, RoleWithPermissionsDto>()
            .Map(
                dest => dest.Permissions,
                src =>
                    src.RolePermissions.Select(rp => rp.Permission)
                        .ToList()
                        .Adapt<IReadOnlyCollection<PermissionDto>>(config)
            );
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
    /// Maps a collection of UserRoleEntity to a read-only collection of RoleDto.
    /// </summary>
    /// <param name="userRoles">The user roles to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A read-only collection of RoleDto.</returns>
    public static IReadOnlyCollection<RoleDto> ToRoleDtos(this ICollection<UserRoleEntity> userRoles, IMapper mapper)
    {
        return userRoles.Select(ur => ur.Role.ToRoleDto(mapper)).ToList();
    }

    /// <summary>
    /// Maps a collection of UserRoleEntity to a deduplicated read-only collection of PermissionDto.
    /// </summary>
    /// <param name="userRoles">The user roles to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A read-only collection of unique PermissionDto across all roles.</returns>
    public static IReadOnlyCollection<PermissionDto> ToPermissionDtos(
        this ICollection<UserRoleEntity> userRoles,
        IMapper mapper
    )
    {
        return userRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.ToPermissionDto(mapper))
            .DistinctBy(p => p.Id)
            .ToList();
    }

    /// <summary>
    /// Maps a RoleEntity with its permissions to a RoleWithPermissionsDto.
    /// </summary>
    /// <param name="role">The role entity with permissions loaded.</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>A RoleWithPermissionsDto containing role and permission information.</returns>
    public static RoleWithPermissionsDto ToRoleWithPermissionsDto(this RoleEntity role, IMapper mapper)
    {
        var dto = mapper.Map<RoleWithPermissionsDto>(role);
        return dto with { Permissions = role.RolePermissions.ToPermissionDtos(mapper) };
    }
}
