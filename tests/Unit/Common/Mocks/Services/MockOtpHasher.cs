using _116.Identity.Application.Auth.Services;
using _116.Tests.Fixtures.Constants;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IOtpHasher"/>.
/// </summary>
public static class MockOtpHasher
{
    /// <summary>
    /// Creates a new mock instance of IOtpHasher.
    /// </summary>
    /// <returns>A configured Mock of IOtpHasher.</returns>
    public static Mock<IOtpHasher> Create()
    {
        Mock<IOtpHasher> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up Hash to return the specified hash for the specified code.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="code">The plaintext code to hash.</param>
    /// <param name="hash">The hash to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpHasher> SetupHash(this Mock<IOtpHasher> mock, string code, string hash)
    {
        mock.Setup(x => x.Hash(code)).Returns(hash);
        return mock;
    }

    /// <summary>
    /// Sets up Hash to return a single hash for any code.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="hash">The hash to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpHasher> SetupHashReturns(this Mock<IOtpHasher> mock, string hash)
    {
        mock.Setup(x => x.Hash(It.IsAny<string>())).Returns(hash);
        return mock;
    }

    /// <summary>
    /// Sets up Verify to accept the specified code against any stored hash.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="code">The code that should verify.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpHasher> SetupVerifySuccess(this Mock<IOtpHasher> mock, string code)
    {
        mock.Setup(x => x.Verify(code, It.IsAny<string?>())).Returns(true);
        return mock;
    }

    /// <summary>
    /// Sets up Verify to reject the specified code against any stored hash.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="code">The code that should not verify.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpHasher> SetupVerifyFailure(this Mock<IOtpHasher> mock, string code)
    {
        mock.Setup(x => x.Verify(code, It.IsAny<string?>())).Returns(false);
        return mock;
    }

    /// <summary>
    /// Verifies that Hash was called with the specified code.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="code">The expected plaintext code.</param>
    public static void VerifyHashCalled(this Mock<IOtpHasher> mock, string code)
    {
        mock.Verify(x => x.Hash(code), Times.Once);
    }

    /// <summary>
    /// Verifies that Verify was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyVerifyNotCalled(this Mock<IOtpHasher> mock)
    {
        mock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    /// <summary>
    /// Defaults hashing to a single stored value and verification to <c>false</c>, so a test that
    /// depends on a code being accepted names it through <see cref="SetupVerifySuccess" />.
    /// </summary>
    /// <param name="mock">The hasher mock to configure.</param>
    private static void SetupDefaults(Mock<IOtpHasher> mock)
    {
        mock.Setup(x => x.Hash(It.IsAny<string>())).Returns(TestConstants.Otp.DefaultCodeHash);

        mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(false);
    }
}
