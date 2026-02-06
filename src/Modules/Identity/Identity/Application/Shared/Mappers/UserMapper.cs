using _116.Core.Application.Shared.DTOs;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Identity.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for User entity mappings.
/// Uses dependency injection instead of global static state.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Registers User entity mappings into the provided TypeAdapterConfig.
    /// This method does NOT mutate global state.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        // Configure UserEntity to UserResponseDto
        config
            .NewConfig<UserEntity, UserResponseDto>()
            .Map(dest => dest.Roles, _ => new List<RoleDto>())
            .Map(dest => dest.Permissions, _ => new List<PermissionDto>())
            .Map(dest => dest.Avatar, _ => (FileDto?)null)
            .Map(dest => dest.AuthProvider, src => src.AuthProvider.ToString());
    }

    /// <summary>
    /// High-performance extension method to map UserEntity to UserResponseDto with roles, permissions, and avatar.
    /// </summary>
    /// <param name="user">User entity to map</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <param name="roles">User roles collection</param>
    /// <param name="permissions">User permissions collection</param>
    /// <param name="avatar">User avatar file information</param>
    /// <returns>Fully populated UserResponseDto</returns>
    public static UserResponseDto ToUserResponseDto(
        this UserEntity user,
        IMapper mapper,
        IReadOnlyCollection<RoleDto> roles,
        IReadOnlyCollection<PermissionDto> permissions,
        FileDto? avatar = null
    )
    {
        var dto = mapper.Map<UserResponseDto>(user);
        return dto with { Roles = roles, Permissions = permissions, Avatar = avatar };
    }

    /// <summary>
    /// Maps FileEntity to FileDto.
    /// </summary>
    /// <param name="fileEntity">The file entity to map</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>Mapped FileDto or null if input is null</returns>
    public static FileDto? ToFileDto(this FileEntity? fileEntity, IMapper mapper)
    {
        return fileEntity == null ? null : mapper.Map<FileDto>(fileEntity);
    }
}
