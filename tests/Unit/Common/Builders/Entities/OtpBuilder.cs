using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Constants;
using Bogus;

namespace _116.Unit.Tests.Common.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="OtpEntity"/> instances in tests.
/// </summary>
public class OtpBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private Guid _userId;
    private string _code;
    private EnumOtpPurpose _purpose = EnumOtpPurpose.EmailVerification;
    private DateTime _expiresAt;
    private int _attemptCount;
    private bool _isUsed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OtpBuilder"/> class with random default values.
    /// </summary>
    public OtpBuilder()
    {
        _id = Guid.NewGuid();
        _userId = Guid.NewGuid();
        _code = _faker.Random.Number(100000, 999999).ToString();
        _expiresAt = DateTime.UtcNow.AddMinutes(TestConstants.Otp.ExpirationMinutes);
    }

    /// <summary>
    /// Sets the OTP ID.
    /// </summary>
    /// <param name="id">The OTP identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the OTP code.
    /// </summary>
    /// <param name="code">The OTP code.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    /// <summary>
    /// Sets the OTP purpose.
    /// </summary>
    /// <param name="purpose">The OTP purpose.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder WithPurpose(EnumOtpPurpose purpose)
    {
        _purpose = purpose;
        return this;
    }

    /// <summary>
    /// Configures the OTP for email verification.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder ForEmailVerification()
    {
        _purpose = EnumOtpPurpose.EmailVerification;
        return this;
    }

    /// <summary>
    /// Configures the OTP for password reset.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder ForPasswordReset()
    {
        _purpose = EnumOtpPurpose.PasswordReset;
        return this;
    }

    /// <summary>
    /// Sets the expiration date.
    /// </summary>
    /// <param name="expiresAt">The expiration date.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder WithExpiresAt(DateTime expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    /// <summary>
    /// Sets the OTP as expired.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder AsExpired()
    {
        _expiresAt = DateTime.UtcNow.AddMinutes(-1);
        return this;
    }

    /// <summary>
    /// Sets the attempt count.
    /// </summary>
    /// <param name="attemptCount">The number of attempts.</param>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder WithAttemptCount(int attemptCount)
    {
        _attemptCount = attemptCount;
        return this;
    }

    /// <summary>
    /// Sets the OTP as having reached max attempts.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder AsMaxAttemptsReached()
    {
        _attemptCount = TestConstants.Otp.MaxAttempts;
        return this;
    }

    /// <summary>
    /// Marks the OTP as used.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public OtpBuilder AsUsed()
    {
        _isUsed = true;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="OtpEntity"/> instance.
    /// </summary>
    /// <returns>A configured OtpEntity instance.</returns>
    public OtpEntity Build()
    {
        OtpEntity otp = OtpEntity.Create(_id, _userId, _code, _purpose, _expiresAt);

        for (int i = 0; i < _attemptCount; i++)
        {
            otp.IncrementAttemptCount();
        }

        if (_isUsed)
        {
            otp.MarkAsUsed();
        }

        return otp;
    }
}
