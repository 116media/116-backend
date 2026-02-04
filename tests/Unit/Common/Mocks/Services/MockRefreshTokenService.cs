using _116.Identity.Application.Auth.Services;
using _116.Unit.Tests.Common.Constants;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IRefreshTokenService"/>.
/// </summary>
public static class MockRefreshTokenService
{
    /// <summary>
    /// Creates a new mock instance of IRefreshTokenService.
    /// </summary>
    /// <returns>A configured Mock of IRefreshTokenService.</returns>
    public static Mock<IRefreshTokenService> Create()
    {
        Mock<IRefreshTokenService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up GenerateRefreshToken to return the specified token.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="token">The refresh token to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRefreshTokenService> SetupGenerateRefreshToken(
        this Mock<IRefreshTokenService> mock,
        string token
    )
    {
        mock.Setup(x => x.GenerateRefreshToken()).Returns(token);
        return mock;
    }

    /// <summary>
    /// Sets up HashRefreshToken to return the specified hash.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="token">The refresh token to hash.</param>
    /// <param name="hash">The hash to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRefreshTokenService> SetupHashRefreshToken(
        this Mock<IRefreshTokenService> mock,
        string token,
        string hash
    )
    {
        mock.Setup(x => x.HashRefreshToken(token)).Returns(hash);
        return mock;
    }

    /// <summary>
    /// Sets up HashRefreshToken to return a default hash for any token.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="hash">The hash to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRefreshTokenService> SetupHashRefreshTokenReturns(
        this Mock<IRefreshTokenService> mock,
        string hash
    )
    {
        mock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns(hash);
        return mock;
    }

    /// <summary>
    /// Verifies that GenerateRefreshToken was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGenerateRefreshTokenCalled(this Mock<IRefreshTokenService> mock)
    {
        mock.Verify(x => x.GenerateRefreshToken(), Times.Once);
    }

    /// <summary>
    /// Verifies that GenerateRefreshToken was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGenerateRefreshTokenNotCalled(this Mock<IRefreshTokenService> mock)
    {
        mock.Verify(x => x.GenerateRefreshToken(), Times.Never);
    }

    /// <summary>
    /// Verifies that HashRefreshToken was called with the specified token.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="token">The expected refresh token.</param>
    public static void VerifyHashRefreshTokenCalled(this Mock<IRefreshTokenService> mock, string token)
    {
        mock.Verify(x => x.HashRefreshToken(token), Times.Once);
    }

    /// <summary>
    /// Verifies that HashRefreshToken was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyHashRefreshTokenCalled(this Mock<IRefreshTokenService> mock)
    {
        mock.Verify(x => x.HashRefreshToken(It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IRefreshTokenService> mock)
    {
        mock.Setup(x => x.GenerateRefreshToken()).Returns(TestConstants.Session.DefaultRefreshToken);

        mock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns(TestConstants.Session.DefaultRefreshTokenHash);
    }
}
