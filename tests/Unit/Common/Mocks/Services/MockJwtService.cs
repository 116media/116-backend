using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Tests.Fixtures.Constants;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IJwtService"/>.
/// </summary>
public static class MockJwtService
{
    /// <summary>
    /// Creates a new mock instance of IJwtService.
    /// </summary>
    /// <returns>A configured Mock of IJwtService.</returns>
    public static Mock<IJwtService> Create()
    {
        Mock<IJwtService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up GenerateToken to return the specified result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="result">The JWT generation result to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IJwtService> SetupGenerateToken(this Mock<IJwtService> mock, JwtGenerationResult result)
    {
        mock.Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(result);
        return mock;
    }

    /// <summary>
    /// Sets up GenerateToken to return a result with the specified token.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="token">The JWT token to return.</param>
    /// <param name="expiresAt">The expiration time (defaults to 1 hour from now).</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IJwtService> SetupGenerateTokenWithValue(
        this Mock<IJwtService> mock,
        string token,
        DateTime? expiresAt = null
    )
    {
        JwtGenerationResult result = new(token, expiresAt ?? DateTime.UtcNow.AddHours(1));
        return mock.SetupGenerateToken(result);
    }

    /// <summary>
    /// Verifies that GenerateToken was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGenerateTokenCalled(this Mock<IJwtService> mock)
    {
        mock.Verify(
            x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that GenerateToken was called with the specified user ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The expected user ID.</param>
    public static void VerifyGenerateTokenCalledWithUserId(this Mock<IJwtService> mock, Guid userId)
    {
        mock.Verify(
            x =>
                x.GenerateToken(
                    userId,
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that GenerateToken was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGenerateTokenNotCalled(this Mock<IJwtService> mock)
    {
        mock.Verify(
            x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                ),
            Times.Never
        );
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IJwtService> mock)
    {
        JwtGenerationResult defaultResult = new(
            TestConstants.Jwt.ValidAccessToken,
            DateTime.UtcNow.AddMinutes(TestConstants.Jwt.AccessTokenExpirationMinutes)
        );

        mock.Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(defaultResult);
    }
}
