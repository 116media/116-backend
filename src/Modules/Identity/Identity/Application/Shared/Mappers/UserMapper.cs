using _116.Core.Contracts.Application.DTOs;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
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
        // The value objects unwrap to their primitives wherever they are mapped, so every DTO
        // keeps its wire contract; record-constructor mapping resolves through these.
        config.NewConfig<Email, string>().MapWith(email => email.Value);
        config.NewConfig<OtpPurpose, EnumOtpPurpose>().MapWith(purpose => purpose.Value);
        config.NewConfig<Client, EnumClient>().MapWith(client => client.Value);

        // Configure UserEntity to UserResponseDto
        config
            .NewConfig<UserEntity, UserResponseDto>()
            .Map(dest => dest.Email, src => src.Email == null ? null : src.Email.Value)
            .Map(dest => dest.Roles, _ => new List<RoleDto>())
            .Map(dest => dest.Permissions, _ => new List<PermissionDto>())
            .Map(dest => dest.Avatar, _ => (FileDto?)null);
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
    /// Projects a mapped <see cref="UserResponseDto" /> to its public shape, dropping the audit
    /// trail and role/permission lifecycle state.
    /// </summary>
    public static PublicUserResponseDto ToPublicUserResponseDto(this UserResponseDto dto)
    {
        return new PublicUserResponseDto(
            dto.Id,
            dto.Email,
            dto.UserName,
            dto.Roles.Select(role => role.ToPublicRoleDto()).ToList(),
            dto.Permissions.Select(permission => permission.ToPublicPermissionDto()).ToList(),
            dto.AuthProvider,
            dto.IsVerified,
            dto.IsActive,
            dto.Avatar.ToPublicFileDto(),
            dto.CountryName,
            dto.CountryIsoCode,
            dto.CountryDialCode,
            dto.PartialPhoneNumber,
            dto.FullPhoneNumber
        );
    }

    /// <summary>
    /// Projects a mapped <see cref="RoleDto" /> to its public shape.
    /// </summary>
    public static PublicRoleDto ToPublicRoleDto(this RoleDto dto)
    {
        return new PublicRoleDto(dto.Id, dto.Name, dto.Description);
    }

    /// <summary>
    /// Projects a mapped <see cref="PermissionDto" /> to its public shape.
    /// </summary>
    public static PublicPermissionDto ToPublicPermissionDto(this PermissionDto dto)
    {
        return new PublicPermissionDto(dto.Id, dto.Resource, dto.Action, dto.Description);
    }

    /// <summary>
    /// Projects a mapped <see cref="RoleWithPermissionsDto" /> to its public shape.
    /// </summary>
    public static PublicRoleWithPermissionsDto ToPublicRoleWithPermissionsDto(this RoleWithPermissionsDto dto)
    {
        return new PublicRoleWithPermissionsDto(
            dto.Id,
            dto.Name,
            dto.Description,
            dto.Permissions.Select(permission => permission.ToPublicPermissionDto()).ToList()
        );
    }

    /// <summary>
    /// Projects a mapped <see cref="FileDto" /> to its public shape, or null.
    /// </summary>
    public static PublicFileDto? ToPublicFileDto(this FileDto? dto)
    {
        return dto is null ? null : new PublicFileDto(dto.Id, dto.StorageUrl, dto.MimeType);
    }

    /// <summary>
    /// Projects a mapped <see cref="SessionDto" /> to its public shape, using the session's
    /// creation instant as the display start time.
    /// </summary>
    public static PublicSessionDto ToPublicSessionDto(this SessionDto dto)
    {
        return new PublicSessionDto(
            dto.Id,
            dto.IpAddress,
            dto.UserAgent,
            dto.Browser,
            dto.Device,
            dto.Platform,
            dto.Client,
            dto.CreatedAt,
            dto.ExpiresAt,
            dto.IsActive,
            dto.IsCurrent
        );
    }
}
