using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;

namespace _116.Unit.Tests.Common.Mothers;

/// <summary>
/// Provides predefined <see cref="OtpEntity"/> scenarios for testing.
/// </summary>
public static class OtpMother
{
    /// <summary>
    /// A valid OTP for email verification.
    /// </summary>
    public static OtpEntity ValidEmailVerification() =>
        new OtpBuilder().WithCode(TestConstants.Otp.ValidCode).ForEmailVerification().Build();

    /// <summary>
    /// A valid OTP for email verification for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static OtpEntity ValidEmailVerification(Guid userId) =>
        new OtpBuilder().WithUserId(userId).WithCode(TestConstants.Otp.ValidCode).ForEmailVerification().Build();

    /// <summary>
    /// A valid OTP for password reset.
    /// </summary>
    public static OtpEntity ValidPasswordReset() =>
        new OtpBuilder().WithCode(TestConstants.Otp.ValidCode).ForPasswordReset().Build();

    /// <summary>
    /// A valid OTP for password reset for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static OtpEntity ValidPasswordReset(Guid userId) =>
        new OtpBuilder().WithUserId(userId).WithCode(TestConstants.Otp.ValidCode).ForPasswordReset().Build();

    /// <summary>
    /// An expired OTP.
    /// </summary>
    public static OtpEntity Expired() => new OtpBuilder().AsExpired().Build();

    /// <summary>
    /// An expired OTP for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static OtpEntity Expired(Guid userId) => new OtpBuilder().WithUserId(userId).AsExpired().Build();

    /// <summary>
    /// A used OTP.
    /// </summary>
    public static OtpEntity Used() => new OtpBuilder().AsUsed().Build();

    /// <summary>
    /// A used OTP for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static OtpEntity Used(Guid userId) => new OtpBuilder().WithUserId(userId).AsUsed().Build();

    /// <summary>
    /// An OTP with max attempts reached.
    /// </summary>
    public static OtpEntity MaxAttemptsReached() => new OtpBuilder().AsMaxAttemptsReached().Build();

    /// <summary>
    /// An OTP with max attempts reached for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static OtpEntity MaxAttemptsReached(Guid userId) =>
        new OtpBuilder().WithUserId(userId).AsMaxAttemptsReached().Build();

    /// <summary>
    /// An OTP about to expire (1 minute remaining).
    /// </summary>
    public static OtpEntity AboutToExpire() => new OtpBuilder().WithExpiresAt(DateTime.UtcNow.AddMinutes(1)).Build();

    /// <summary>
    /// An OTP with some failed attempts.
    /// </summary>
    public static OtpEntity WithFailedAttempts(int attemptCount) =>
        new OtpBuilder().WithAttemptCount(attemptCount).Build();

    /// <summary>
    /// A valid OTP with test constants.
    /// </summary>
    public static OtpEntity Valid() => new OtpBuilder().WithCode(TestConstants.Otp.ValidCode).Build();

    /// <summary>
    /// A random OTP for general testing.
    /// </summary>
    public static OtpEntity Random() => new OtpBuilder().Build();

    /// <summary>
    /// An invalid OTP (used, expired, or max attempts).
    /// </summary>
    public static OtpEntity Invalid() => new OtpBuilder().AsExpired().AsUsed().AsMaxAttemptsReached().Build();
}
