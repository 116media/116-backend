using _116.Identity.Application.Auth.Services;
using _116.Unit.Tests.Common.Constants;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IPasswordService"/>.
/// </summary>
public static class MockPasswordService
{
    /// <summary>
    /// Creates a new mock instance of IPasswordService.
    /// </summary>
    /// <returns>A configured Mock of IPasswordService.</returns>
    public static Mock<IPasswordService> Create()
    {
        Mock<IPasswordService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up Hash to return the specified hash value.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="password">The password to hash.</param>
    /// <param name="hash">The hash to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPasswordService> SetupHash(this Mock<IPasswordService> mock, string password, string hash)
    {
        mock.Setup(x => x.Hash(password)).Returns(hash);
        return mock;
    }

    /// <summary>
    /// Sets up Hash to return a default hash for any password.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="hash">The hash to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPasswordService> SetupHashReturns(this Mock<IPasswordService> mock, string hash)
    {
        mock.Setup(x => x.Hash(It.IsAny<string>())).Returns(hash);
        return mock;
    }

    /// <summary>
    /// Sets up Verify to return true for the specified password and hash.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to verify against.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPasswordService> SetupVerifySuccess(
        this Mock<IPasswordService> mock,
        string password,
        string hash
    )
    {
        mock.Setup(x => x.Verify(password, hash)).Returns(true);
        return mock;
    }

    /// <summary>
    /// Sets up Verify to return false for the specified password and hash.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to verify against.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPasswordService> SetupVerifyFailure(
        this Mock<IPasswordService> mock,
        string password,
        string hash
    )
    {
        mock.Setup(x => x.Verify(password, hash)).Returns(false);
        return mock;
    }

    /// <summary>
    /// Sets up Verify to return true for any password and hash.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPasswordService> SetupVerifyReturnsTrue(this Mock<IPasswordService> mock)
    {
        mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(true);
        return mock;
    }

    /// <summary>
    /// Sets up Verify to return false for any password and hash.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPasswordService> SetupVerifyReturnsFalse(this Mock<IPasswordService> mock)
    {
        mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(false);
        return mock;
    }

    /// <summary>
    /// Verifies that Hash was called with the specified password.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="password">The expected password.</param>
    public static void VerifyHashCalled(this Mock<IPasswordService> mock, string password)
    {
        mock.Verify(x => x.Hash(password), Times.Once);
    }

    /// <summary>
    /// Verifies that Hash was called with any password.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyHashCalled(this Mock<IPasswordService> mock)
    {
        mock.Verify(x => x.Hash(It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Verify was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyVerifyCalled(this Mock<IPasswordService> mock)
    {
        mock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Verify was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyVerifyNotCalled(this Mock<IPasswordService> mock)
    {
        mock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IPasswordService> mock)
    {
        mock.Setup(x => x.Hash(It.IsAny<string>())).Returns(TestConstants.User.DefaultPasswordHash);

        mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(true);
    }
}
