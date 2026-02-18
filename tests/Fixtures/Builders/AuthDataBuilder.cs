using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Factories;

namespace _116.Tests.Fixtures.Builders;

/// <summary>
/// Fluent builder for creating auth data structures in tests.
/// Combines random data generation (via UserFactory) with customization options.
/// </summary>
public class AuthDataBuilder
{
    private UserEntity _user;
    private List<RolePermissionEntity> _userPermissions;
    private List<RoleDto> _roles;
    private List<PermissionDto> _permissions;

    /// <summary>
    /// Initializes a new instance with default values using random user data.
    /// </summary>
    public AuthDataBuilder()
    {
        // Use factory for random verified active user (Bogus-generated email/username)
        _user = UserFactory.CreateVerifiedActive();
        _userPermissions = [];
        _roles = [];
        _permissions = [];
    }

    /// <summary>
    /// Initializes with a specific user (for predictable scenarios).
    /// </summary>
    /// <param name="user">The user entity to use.</param>
    public AuthDataBuilder(UserEntity user)
    {
        _user = user;
        _userPermissions = [];
        _roles = [];
        _permissions = [];
    }

    /// <summary>
    /// Sets the user for the auth data.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuthDataBuilder WithUser(UserEntity user)
    {
        _user = user;
        return this;
    }

    /// <summary>
    /// Sets the user permissions list.
    /// </summary>
    /// <param name="permissions">The list of user permissions.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuthDataBuilder WithUserPermissions(List<RolePermissionEntity> permissions)
    {
        _userPermissions = permissions;
        return this;
    }

    /// <summary>
    /// Adds a single user permission.
    /// </summary>
    /// <param name="permission">The permission to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuthDataBuilder WithUserPermission(RolePermissionEntity permission)
    {
        _userPermissions.Add(permission);
        return this;
    }

    /// <summary>
    /// Sets the role list.
    /// </summary>
    /// <param name="roles">The list of role DTOs.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuthDataBuilder WithRoles(List<RoleDto> roles)
    {
        _roles = roles;
        return this;
    }

    /// <summary>
    /// Adds a single role DTO.
    /// </summary>
    /// <param name="role">The role DTO to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuthDataBuilder WithRole(RoleDto role)
    {
        _roles.Add(role);
        return this;
    }

    /// <summary>
    /// Sets the permissions list.
    /// </summary>
    /// <param name="permissions">The list of permission DTOs.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuthDataBuilder WithPermissions(List<PermissionDto> permissions)
    {
        _permissions = permissions;
        return this;
    }

    /// <summary>
    /// Adds a single permission DTO.
    /// </summary>
    /// <param name="permission">The permission DTO to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AuthDataBuilder WithPermission(PermissionDto permission)
    {
        _permissions.Add(permission);
        return this;
    }

    /// <summary>
    /// Builds PublicLoginAuthData instance.
    /// </summary>
    /// <returns>A configured PublicLoginAuthData instance.</returns>
    public PublicLoginAuthData BuildPublicLoginAuthData()
    {
        return new PublicLoginAuthData(_user, _userPermissions, _roles, _permissions);
    }

    /// <summary>
    /// Builds PublicSocialLoginAuthData instance.
    /// </summary>
    /// <returns>A configured PublicSocialLoginAuthData instance.</returns>
    public PublicSocialLoginAuthData BuildPublicSocialLoginAuthData()
    {
        return new PublicSocialLoginAuthData(_user, _userPermissions, _roles, _permissions);
    }

    /// <summary>
    /// Builds AdminLoginAuthData instance.
    /// </summary>
    /// <returns>A configured AdminLoginAuthData instance.</returns>
    public AdminLoginAuthData BuildAdminLoginAuthData()
    {
        return new AdminLoginAuthData(_user, _userPermissions, _roles, _permissions);
    }
}
