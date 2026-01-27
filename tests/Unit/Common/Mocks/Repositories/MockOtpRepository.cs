using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IOtpRepository"/>.
/// </summary>
public static class MockOtpRepository
{
    /// <summary>
    /// Creates a new mock instance of IOtpRepository.
    /// </summary>
    /// <returns>A configured Mock of IOtpRepository.</returns>
    public static Mock<IOtpRepository> Create()
    {
        Mock<IOtpRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up ValidateOtpAsync to return the specified OTP entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="otp">The OTP entity to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpRepository> SetupValidateOtp(this Mock<IOtpRepository> mock, OtpEntity otp)
    {
        mock.Setup(x => x.ValidateOtpAsync(otp.UserId, otp.Code, otp.Purpose, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otp);
        return mock;
    }

    /// <summary>
    /// Sets up ValidateOtpAsync to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpRepository> SetupValidateOtpNotFound(
        this Mock<IOtpRepository> mock,
        Guid userId,
        string code,
        EnumOtpPurpose purpose
    )
    {
        mock.Setup(x => x.ValidateOtpAsync(userId, code, purpose, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("OTP not found."));
        return mock;
    }

    /// <summary>
    /// Sets up ValidateOtpAsync to throw BadRequestException for invalid code.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpRepository> SetupValidateOtpInvalidCode(
        this Mock<IOtpRepository> mock,
        Guid userId,
        string code,
        EnumOtpPurpose purpose
    )
    {
        mock.Setup(x => x.ValidateOtpAsync(userId, code, purpose, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid OTP code."));
        return mock;
    }

    /// <summary>
    /// Sets up ValidateOtpAsync to throw AuthenticationException for expired OTP.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpRepository> SetupValidateOtpExpired(
        this Mock<IOtpRepository> mock,
        Guid userId,
        string code,
        EnumOtpPurpose purpose
    )
    {
        mock.Setup(x => x.ValidateOtpAsync(userId, code, purpose, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthenticationException("OTP has expired."));
        return mock;
    }

    /// <summary>
    /// Sets up ValidateUsedOtpAsync to return the specified OTP entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="otp">The OTP entity to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpRepository> SetupValidateUsedOtp(this Mock<IOtpRepository> mock, OtpEntity otp)
    {
        mock.Setup(x => x.ValidateUsedOtpAsync(otp.UserId, otp.Code, otp.Purpose, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otp);
        return mock;
    }

    /// <summary>
    /// Sets up InvalidateExistingOtpsAsync to complete successfully.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpRepository> SetupInvalidateExistingOtps(
        this Mock<IOtpRepository> mock,
        Guid userId,
        EnumOtpPurpose purpose
    )
    {
        mock.Setup(x => x.InvalidateExistingOtpsAsync(userId, purpose, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Sets up CleanupExpiredOtpsAsync to return the specified count.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="count">The number of expired OTPs removed.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IOtpRepository> SetupCleanupExpiredOtps(this Mock<IOtpRepository> mock, int count)
    {
        mock.Setup(x => x.CleanupExpiredOtpsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(count);
        return mock;
    }

    /// <summary>
    /// Verifies that AddAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyOtp">Optional predicate to verify the OTP.</param>
    public static void VerifyAddCalled(this Mock<IOtpRepository> mock, Func<OtpEntity, bool>? verifyOtp = null)
    {
        if (verifyOtp is not null)
        {
            mock.Verify(
                x => x.AddAsync(It.Is<OtpEntity>(o => verifyOtp(o)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.AddAsync(It.IsAny<OtpEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that InvalidateExistingOtpsAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The expected user ID.</param>
    /// <param name="purpose">The expected OTP purpose.</param>
    public static void VerifyInvalidateExistingOtpsCalled(
        this Mock<IOtpRepository> mock,
        Guid userId,
        EnumOtpPurpose purpose
    )
    {
        mock.Verify(x => x.InvalidateExistingOtpsAsync(userId, purpose, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IOtpRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<OtpEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        mock.Setup(x =>
                x.InvalidateExistingOtpsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<EnumOtpPurpose>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);
    }
}
