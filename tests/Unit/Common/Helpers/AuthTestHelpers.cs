using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders;
using _116.Unit.Tests.Common.Factories;

namespace _116.Unit.Tests.Common.Helpers;

/// <summary>
/// Shared test helpers for authentication and session-related tests.
/// Provides both random and predictable test data generation.
/// </summary>
public static class AuthTestHelpers
{
    /// <summary>
    /// Creates a default SessionResult for testing purposes.
    /// </summary>
    public static SessionResult CreateDefaultSessionResult()
    {
        return new SessionResult(
            RefreshToken: "refresh-token",
            AccessToken: "access-token",
            AccessTokenExpiresAt: DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(7)
        );
    }

    /// <summary>
    /// Creates a RoleDto for testing purposes.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description.</param>
    /// <param name="isActive">Whether the role is active.</param>
    /// <param name="isDeleted">Whether the role is deleted.</param>
    /// <returns>A configured RoleDto instance.</returns>
    public static RoleDto CreateRoleDto(
        string name = "Admin",
        string description = "Administrator role",
        bool isActive = true,
        bool isDeleted = false
    )
    {
        return new RoleDto(
            Id: Guid.NewGuid(),
            Name: name,
            Description: description,
            IsActive: isActive,
            IsDeleted: isDeleted,
            DeletedAt: isDeleted ? DateTime.UtcNow : null
        );
    }

    /// <summary>
    /// Creates a PermissionDto for testing purposes.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <param name="action">The action type.</param>
    /// <param name="description">The permission description.</param>
    /// <param name="isActive">Whether the permission is active.</param>
    /// <param name="isDeleted">Whether the permission is deleted.</param>
    /// <returns>A configured PermissionDto instance.</returns>
    public static PermissionDto CreatePermissionDto(
        string resource = "users",
        string action = "read",
        string description = "Read users",
        bool isActive = true,
        bool isDeleted = false
    )
    {
        return new PermissionDto(
            Id: Guid.NewGuid(),
            Resource: resource,
            Action: action,
            Description: description,
            IsActive: isActive,
            IsDeleted: isDeleted,
            DeletedAt: isDeleted ? DateTime.UtcNow : null
        );
    }

    /// <summary>
    /// Creates PublicLoginAuthData with random user data (via UserFactory).
    /// </summary>
    /// <returns>PublicLoginAuthData with random verified active user.</returns>
    public static PublicLoginAuthData CreatePublicLoginAuthData() => new AuthDataBuilder().BuildPublicLoginAuthData();

    /// <summary>
    /// Creates PublicLoginAuthData with a specific user (predictable scenario).
    /// </summary>
    /// <param name="user">The user entity to use.</param>
    /// <returns>PublicLoginAuthData with the specified user.</returns>
    public static PublicLoginAuthData CreatePublicLoginAuthData(UserEntity user) =>
        new AuthDataBuilder(user).BuildPublicLoginAuthData();

    /// <summary>
    /// Creates PublicSocialLoginAuthData with random user data.
    /// </summary>
    /// <returns>PublicSocialLoginAuthData with random verified active user.</returns>
    public static PublicSocialLoginAuthData CreatePublicSocialLoginAuthData() =>
        new AuthDataBuilder().BuildPublicSocialLoginAuthData();

    /// <summary>
    /// Creates PublicSocialLoginAuthData with a specific user.
    /// </summary>
    /// <param name="user">The user entity to use.</param>
    /// <returns>PublicSocialLoginAuthData with the specified user.</returns>
    public static PublicSocialLoginAuthData CreatePublicSocialLoginAuthData(UserEntity user) =>
        new AuthDataBuilder(user).BuildPublicSocialLoginAuthData();

    /// <summary>
    /// Creates AdminLoginAuthData with random user data.
    /// </summary>
    /// <returns>AdminLoginAuthData with random verified active user.</returns>
    public static AdminLoginAuthData CreateAdminLoginAuthData() => new AuthDataBuilder().BuildAdminLoginAuthData();

    /// <summary>
    /// Creates AdminLoginAuthData with a specific user (e.g., UserFactory.CreateSuperAdmin()).
    /// </summary>
    /// <param name="user">The user entity to use.</param>
    /// <returns>AdminLoginAuthData with the specified user.</returns>
    public static AdminLoginAuthData CreateAdminLoginAuthData(UserEntity user) =>
        new AuthDataBuilder(user).BuildAdminLoginAuthData();

    /// <summary>
    /// Creates AdminLoginAuthData for SuperAdmin (predictable email: superadmin@116.com).
    /// </summary>
    /// <returns>AdminLoginAuthData with SuperAdmin user.</returns>
    public static AdminLoginAuthData CreateSuperAdminLoginAuthData() =>
        new AuthDataBuilder(UserFactory.CreateSuperAdmin()).BuildAdminLoginAuthData();
}
