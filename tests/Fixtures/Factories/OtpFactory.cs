using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories;

/// <summary>
/// Factory for quickly creating <see cref="OtpEntity"/> instances in tests.
/// </summary>
public static class OtpFactory
{
    /// <summary>
    /// Creates an OTP with default random values.
    /// </summary>
    /// <returns>A new OtpEntity with random values.</returns>
    public static OtpEntity Create() => new OtpBuilder().Build();

    /// <summary>
    /// Creates an OTP for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new OtpEntity for the specified user.</returns>
    public static OtpEntity Create(Guid userId) => new OtpBuilder().WithUserId(userId).Build();

    /// <summary>
    /// Creates an OTP with a specific user and code.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The OTP code.</param>
    /// <returns>A new OtpEntity with the specified values.</returns>
    public static OtpEntity Create(Guid userId, string code) =>
        new OtpBuilder().WithUserId(userId).WithCode(code).Build();

    /// <summary>
    /// Creates an OTP with a specific ID.
    /// </summary>
    /// <param name="id">The OTP identifier.</param>
    /// <returns>A new OtpEntity with the specified ID.</returns>
    public static OtpEntity CreateWithId(Guid id) => new OtpBuilder().WithId(id).Build();

    /// <summary>
    /// Creates an OTP for email verification.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new email verification OtpEntity.</returns>
    public static OtpEntity CreateForEmailVerification(Guid userId) =>
        new OtpBuilder().WithUserId(userId).ForEmailVerification().Build();

    /// <summary>
    /// Creates an OTP for password reset.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new password reset OtpEntity.</returns>
    public static OtpEntity CreateForPasswordReset(Guid userId) =>
        new OtpBuilder().WithUserId(userId).ForPasswordReset().Build();

    /// <summary>
    /// Creates an expired OTP.
    /// </summary>
    /// <returns>A new expired OtpEntity.</returns>
    public static OtpEntity CreateExpired() => new OtpBuilder().AsExpired().Build();

    /// <summary>
    /// Creates an expired OTP for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new expired OtpEntity for the specified user.</returns>
    public static OtpEntity CreateExpired(Guid userId) => new OtpBuilder().WithUserId(userId).AsExpired().Build();

    /// <summary>
    /// Creates a used OTP.
    /// </summary>
    /// <returns>A new used OtpEntity.</returns>
    public static OtpEntity CreateUsed() => new OtpBuilder().AsUsed().Build();

    /// <summary>
    /// Creates a used OTP for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new used OtpEntity for the specified user.</returns>
    public static OtpEntity CreateUsed(Guid userId) => new OtpBuilder().WithUserId(userId).AsUsed().Build();

    /// <summary>
    /// Creates an OTP with max attempts reached.
    /// </summary>
    /// <returns>A new OtpEntity with max attempts.</returns>
    public static OtpEntity CreateMaxAttemptsReached() => new OtpBuilder().AsMaxAttemptsReached().Build();

    /// <summary>
    /// Creates an OTP with max attempts reached for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new OtpEntity with max attempts for the specified user.</returns>
    public static OtpEntity CreateMaxAttemptsReached(Guid userId) =>
        new OtpBuilder().WithUserId(userId).AsMaxAttemptsReached().Build();

    /// <summary>
    /// Creates a valid OTP with a known code.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new valid OtpEntity with a known code.</returns>
    public static OtpEntity CreateValid(Guid userId) =>
        new OtpBuilder().WithUserId(userId).WithCode(TestConstants.Otp.ValidCode).Build();

    /// <summary>
    /// Creates a list of OTPs with the specified count.
    /// </summary>
    /// <param name="count">The number of OTPs to create.</param>
    /// <returns>A list of OtpEntity instances.</returns>
    public static List<OtpEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();

    /// <summary>
    /// Creates an OTP with a specific purpose.
    /// </summary>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new OtpEntity with the specified purpose.</returns>
    public static OtpEntity CreateWithPurpose(EnumOtpPurpose purpose) => new OtpBuilder().WithPurpose(purpose).Build();

    /// <summary>
    /// Creates an OTP for a specific user and purpose.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new OtpEntity with the specified values.</returns>
    public static OtpEntity Create(Guid userId, EnumOtpPurpose purpose) =>
        new OtpBuilder().WithUserId(userId).WithPurpose(purpose).Build();

    /// <summary>
    /// Creates an OTP with a specific user, code, and purpose.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new OtpEntity with the specified values.</returns>
    public static OtpEntity Create(Guid userId, string code, EnumOtpPurpose purpose) =>
        new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).Build();

    /// <summary>
    /// Creates an OTP with a specific code.
    /// </summary>
    /// <param name="code">The OTP code.</param>
    /// <returns>A new OtpEntity with the specified code.</returns>
    public static OtpEntity CreateWithCode(string code) => new OtpBuilder().WithCode(code).Build();

    /// <summary>
    /// Creates an OTP with a specific expiration date.
    /// </summary>
    /// <param name="expiresAt">The expiration date.</param>
    /// <returns>A new OtpEntity with the specified expiration date.</returns>
    public static OtpEntity CreateWithExpiresAt(DateTime expiresAt) =>
        new OtpBuilder().WithExpiresAt(expiresAt).Build();

    /// <summary>
    /// Creates an OTP with a specific attempt count.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <param name="attemptCount">The attempt count.</param>
    /// <returns>A new OtpEntity with the specified values.</returns>
    public static OtpEntity CreateWithAttemptCount(
        Guid userId,
        string code,
        EnumOtpPurpose purpose,
        int attemptCount
    ) => new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).WithAttemptCount(attemptCount).Build();

    /// <summary>
    /// Creates an expired OTP for a specific user and purpose.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new expired OtpEntity for the specified user and purpose.</returns>
    public static OtpEntity CreateExpired(Guid userId, EnumOtpPurpose purpose) =>
        new OtpBuilder().WithUserId(userId).WithPurpose(purpose).AsExpired().Build();

    /// <summary>
    /// Creates a used OTP for a specific user and purpose.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new used OtpEntity for the specified user and purpose.</returns>
    public static OtpEntity CreateUsed(Guid userId, EnumOtpPurpose purpose) =>
        new OtpBuilder().WithUserId(userId).WithPurpose(purpose).AsUsed().Build();

    /// <summary>
    /// Creates an expired OTP for a specific user, code, and purpose.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new expired OtpEntity with the specified values.</returns>
    public static OtpEntity CreateExpired(Guid userId, string code, EnumOtpPurpose purpose) =>
        new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsExpired().Build();

    /// <summary>
    /// Creates an OTP with max attempts reached for a specific user, code, and purpose.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new OtpEntity with max attempts for the specified values.</returns>
    public static OtpEntity CreateMaxAttemptsReached(Guid userId, string code, EnumOtpPurpose purpose) =>
        new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsMaxAttemptsReached().Build();

    /// <summary>
    /// Creates a used OTP for a specific user, code, and purpose.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new used OtpEntity with the specified values.</returns>
    public static OtpEntity CreateUsed(Guid userId, string code, EnumOtpPurpose purpose) =>
        new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsUsed().Build();

    /// <summary>
    /// Creates a used and expired OTP for a specific user, code, and purpose.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>A new used and expired OtpEntity with the specified values.</returns>
    public static OtpEntity CreateUsedAndExpired(Guid userId, string code, EnumOtpPurpose purpose) =>
        new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsUsed().AsExpired().Build();
}
