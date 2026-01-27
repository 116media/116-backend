using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Constants;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IOtpService"/>.
/// </summary>
public static class MockOtpService
{
    /// <summary>
    /// Creates a new mock instance of IOtpService.
    /// </summary>
    /// <returns>A configured Mock of IOtpService.</returns>
    public static Mock<IOtpService> Create()
    {
        Mock<IOtpService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up GenerateOtpCode to return the specified code.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="code">The OTP code to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpService> SetupGenerateOtpCode(this Mock<IOtpService> mock, string code)
    {
        mock.Setup(x => x.GenerateOtpCode()).Returns(code);
        return mock;
    }

    /// <summary>
    /// Sets up CreateOtp to return the specified OTP entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="otp">The OTP entity to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpService> SetupCreateOtp(this Mock<IOtpService> mock, OtpEntity otp)
    {
        mock.Setup(x => x.CreateOtp(otp.UserId, otp.Purpose)).Returns(otp);
        return mock;
    }

    /// <summary>
    /// Sets up CreateOtp to return an OTP for any user ID and purpose.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="otp">The OTP entity to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpService> SetupCreateOtpReturns(this Mock<IOtpService> mock, OtpEntity otp)
    {
        mock.Setup(x => x.CreateOtp(It.IsAny<Guid>(), It.IsAny<EnumOtpPurpose>())).Returns(otp);
        return mock;
    }

    /// <summary>
    /// Sets up CalculateExpirationTime to return the specified time.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="expirationTime">The expiration time to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpService> SetupCalculateExpirationTime(this Mock<IOtpService> mock, DateTime expirationTime)
    {
        mock.Setup(x => x.CalculateExpirationTime()).Returns(expirationTime);
        return mock;
    }

    /// <summary>
    /// Verifies that GenerateOtpCode was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGenerateOtpCodeCalled(this Mock<IOtpService> mock)
    {
        mock.Verify(x => x.GenerateOtpCode(), Times.Once);
    }

    /// <summary>
    /// Verifies that CreateOtp was called with the specified parameters.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The expected user ID.</param>
    /// <param name="purpose">The expected OTP purpose.</param>
    public static void VerifyCreateOtpCalled(this Mock<IOtpService> mock, Guid userId, EnumOtpPurpose purpose)
    {
        mock.Verify(x => x.CreateOtp(userId, purpose), Times.Once);
    }

    /// <summary>
    /// Verifies that CreateOtp was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyCreateOtpCalled(this Mock<IOtpService> mock)
    {
        mock.Verify(x => x.CreateOtp(It.IsAny<Guid>(), It.IsAny<EnumOtpPurpose>()), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IOtpService> mock)
    {
        mock.Setup(x => x.GenerateOtpCode()).Returns(TestConstants.Otp.DefaultCode);

        mock.Setup(x => x.CalculateExpirationTime())
            .Returns(DateTime.UtcNow.AddMinutes(TestConstants.Otp.ExpirationMinutes));
    }
}
