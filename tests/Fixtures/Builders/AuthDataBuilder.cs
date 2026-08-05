using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Domain.Entities;

namespace _116.Tests.Fixtures.Builders;

/// <summary>
/// Fluent builder for the auth-data records the login use cases hand to their factories.
/// Reached through the three AuthTestHelpers aliases rather than named from a test directly.
/// </summary>
public class AuthDataBuilder
{
    private readonly UserEntity _user;
    private readonly List<RolePermissionEntity> _userPermissions;

    /// <summary>
    /// Initializes the builder for a specific user, with no permissions granted.
    /// </summary>
    /// <param name="user">The user entity to use.</param>
    public AuthDataBuilder(UserEntity user)
    {
        _user = user;
        _userPermissions = [];
    }

    /// <summary>
    /// Builds PublicLoginAuthData instance.
    /// </summary>
    /// <returns>A configured PublicLoginAuthData instance.</returns>
    public PublicLoginAuthData BuildPublicLoginAuthData()
    {
        return new PublicLoginAuthData(User: _user, UserPermissions: _userPermissions);
    }

    /// <summary>
    /// Builds PublicSocialLoginAuthData instance.
    /// </summary>
    /// <returns>A configured PublicSocialLoginAuthData instance.</returns>
    public PublicSocialLoginAuthData BuildPublicSocialLoginAuthData()
    {
        return new PublicSocialLoginAuthData(User: _user, UserPermissions: _userPermissions);
    }

    /// <summary>
    /// Builds AdminLoginAuthData instance.
    /// </summary>
    /// <returns>A configured AdminLoginAuthData instance.</returns>
    public AdminLoginAuthData BuildAdminLoginAuthData()
    {
        return new AdminLoginAuthData(User: _user, UserPermissions: _userPermissions);
    }
}
