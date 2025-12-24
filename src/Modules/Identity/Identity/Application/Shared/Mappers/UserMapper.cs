using _116.Core.Application.Shared.DTOs;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;

using Mapster;

namespace _116.Identity.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for User entity mappings.
/// Leverages automatic mapping with minimal manual configuration.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Configures Mapster mappings once at application startup.
    /// Uses automatic property mapping with selective overrides.
    /// </summary>
    public static void Configure()
    {
        // Configure UserEntity to UserResponseDto - leverage automatic mapping for matching properties
        TypeAdapterConfig<UserEntity, UserResponseDto>
            .NewConfig()
            .Map(dest => dest.Roles, _ => new List<RoleDto>())
            .Map(dest => dest.Permissions, _ => new List<PermissionDto>())
            .Map(dest => dest.Avatar, _ => (FileDto?)null) // Null until core file implemented
            .Map(dest => dest.AuthProvider, src => src.AuthProvider.ToString()) // Convert enum to string
            .Compile(); // Compile for performance
    }
    /// <summary>
    /// High-performance extension method to map UserEntity to UserResponseDto with roles, permissions, and avatar.
    /// Leverage Mapster's compiled mappings with selective property override.
    /// </summary>
    /// <param name="user">User entity to map</param>
    /// <param name="roles">User roles collection</param>
    /// <param name="permissions">User permissions collection</param>
    /// <param name="avatar">User avatar file information</param>
    /// <returns>Fully populated UserResponseDto</returns>
    public static UserResponseDto ToUserResponseDto(
        this UserEntity user,
        IReadOnlyCollection<RoleDto> roles,
        IReadOnlyCollection<PermissionDto> permissions,
        FileDto? avatar = null
    )
    {
        // Use base mapping then override specific collections and avatar - avoids repetitive property mapping
        var dto = user.Adapt<UserResponseDto>();
        // Only override the collections and avatar that can't be auto-mapped
        return dto with
        {
            Roles = roles,
            Permissions = permissions,
            Avatar = avatar
        };
    }
    /// <summary>
    /// Maps FileEntity to FileDto.
    /// </summary>
    /// <param name="fileEntity">The file entity to map</param>
    /// <returns>Mapped FileDto or null if input is null</returns>
    public static FileDto? ToFileDto(this FileEntity? fileEntity)
    {
        return fileEntity?.Adapt<FileDto>();
    }
}
