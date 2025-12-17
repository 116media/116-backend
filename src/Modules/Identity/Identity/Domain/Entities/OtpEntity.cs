using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Shared.Domain;
using _116.Auth.Domain.Enums;

namespace _116.Auth.Domain.Entities;

/// <summary>
/// Represents a one-time password (OTP) for user verification.
/// </summary>
public class OtpEntity : Aggregate<Guid>
{
    /// <summary>
    /// Foreign key referencing the associated user.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The OTP code sent to the user.
    /// </summary>
    [MaxLength(UserConstants.OtpCodeLength)]
    public string Code { get; private set; } = null!;

    /// <summary>
    /// The purpose of the OTP (EmailVerification, PasswordReset, etc.).
    /// </summary>
    public EnumOtpPurpose Purpose { get; private set; }

    /// <summary>
    /// The date and time when the OTP expires, in UTC.
    /// </summary>
    public DateTime ExpiresAt { get; private init; }

    /// <summary>
    /// The number of verification attempts made with this OTP.
    /// </summary>
    public int AttemptCount { get; private set; }

    /// <summary>
    /// Indicates whether the OTP has been used successfully.
    /// </summary>
    public bool IsUsed { get; private set; }

    /// <summary>
    /// The date and time when the OTP was used, in UTC.
    /// </summary>
    public DateTime? UsedAt { get; private set; }

    /// <summary>
    /// Navigation property for the associated user.
    /// </summary>
    public UserEntity User { get; private set; } = null!;

    /// <summary>
    /// Creates a new OTP entity.
    /// </summary>
    /// <param name="id">The unique identifier of the OTP.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="code">The OTP code.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="expiresAt">When the OTP expires.</param>
    /// <returns>A new <see cref="OtpEntity"/> instance.</returns>
    public static OtpEntity Create(Guid id, Guid userId, string code, EnumOtpPurpose purpose, DateTime expiresAt)
    {
        return new OtpEntity
        {
            Id = id,
            UserId = userId,
            Code = code,
            Purpose = purpose,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Marks the OTP as used.
    /// </summary>
    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increments the attempt count.
    /// </summary>
    public void IncrementAttemptCount()
    {
        AttemptCount++;
    }

    /// <summary>
    /// Checks if the OTP is valid for verification.
    /// </summary>
    /// <returns>True if the OTP is valid, otherwise false.</returns>
    public bool IsValid()
    {
        return !IsUsed
               && DateTime.UtcNow <= ExpiresAt
               && AttemptCount < UserConstants.MaxOtpAttempts;
    }

    /// <summary>
    /// Checks if the OTP has expired.
    /// </summary>
    /// <returns>True if the OTP has expired, otherwise false.</returns>
    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Checks if the maximum attempts have been reached.
    /// </summary>
    /// <returns>True if maximum attempts reached, otherwise false.</returns>
    public bool HasMaxAttemptsReached()
    {
        return AttemptCount >= UserConstants.MaxOtpAttempts;
    }
}
