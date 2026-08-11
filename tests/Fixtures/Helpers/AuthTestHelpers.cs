using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Builders;

namespace _116.Tests.Fixtures.Helpers;

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
    /// Creates PublicLoginAuthData with a specific user (predictable scenario).
    /// These three aliases are the intended entry point; AuthDataBuilder is an implementation
    /// detail of this file and is not part of the fixture API tests reach for.
    /// </summary>
    public static PublicLoginAuthData CreatePublicLoginAuthData(UserEntity user) =>
        new AuthDataBuilder(user).BuildPublicLoginAuthData();

    /// <summary>
    /// Creates PublicSocialLoginAuthData with a specific user.
    /// </summary>
    public static PublicSocialLoginAuthData CreatePublicSocialLoginAuthData(UserEntity user) =>
        new AuthDataBuilder(user).BuildPublicSocialLoginAuthData();

    /// <summary>
    /// Creates AdminLoginAuthData with a specific user.
    /// </summary>
    public static AdminLoginAuthData CreateAdminLoginAuthData(UserEntity user) =>
        new AuthDataBuilder(user).BuildAdminLoginAuthData();
}
