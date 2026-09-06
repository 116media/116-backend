using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

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
    /// The salted hash of the OTP code that was sent to the user.
    /// The deliverable code itself is never stored; verification compares a supplied
    /// code against this hash.
    /// </summary>
    [MaxLength(length: UserConstants.OtpCodeHashLength)]
    public string CodeHash { get; private set; } = null!;

    /// <summary>
    /// The purpose of the OTP (EmailVerification, PasswordReset, etc.).
    /// </summary>
    public OtpPurpose Purpose { get; private set; } = null!;

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
    /// The date and time at which the code was spent or superseded, in UTC.
    /// </summary>
    /// <remarks>
    /// Distinct from <see cref="IsUsed" />, which only records that the owner verified the code.
    /// A consumed code is never valid again, so superseding a code on resend and spending one on a
    /// password reset cannot be mistaken for a verification.
    /// </remarks>
    public DateTime? ConsumedAt { get; private set; }

    /// <summary>
    /// Navigation property for the associated user.
    /// </summary>
    public UserEntity User { get; private set; } = null!;

    /// <summary>
    /// Creates a new OTP entity.
    /// </summary>
    /// <param name="id">The unique identifier of the OTP.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="codeHash">The salted hash of the OTP code.</param>
    /// <param name="purpose">The purpose of the OTP.</param>
    /// <param name="expiresAt">When the OTP expires.</param>
    /// <returns>A new <see cref="OtpEntity" /> instance.</returns>
    public static OtpEntity Create(Guid id, Guid userId, string codeHash, EnumOtpPurpose purpose, DateTime expiresAt)
    {
        return new OtpEntity
        {
            Id = id,
            UserId = userId,
            CodeHash = codeHash,
            Purpose = new OtpPurpose(value: purpose),
            ExpiresAt = expiresAt,
        };
    }

    /// <summary>
    /// Records that the code still owes the user a delivery, carrying the plaintext the row
    /// itself never stores. Raised before the save so the code is delivered only if the flow
    /// that issued it commits.
    /// </summary>
    /// <param name="plainCode">The code to deliver.</param>
    /// <param name="culture">The culture the delivery should be rendered in.</param>
    public void MarkIssued(string plainCode, string culture)
    {
        AddDomainEvent(new OtpIssuedEvent(UserId: UserId, PlainCode: plainCode, Purpose: Purpose, Culture: culture));
    }

    /// <summary>
    /// Marks the OTP as used. Idempotent: an OTP already used reports <c>false</c> and keeps its
    /// original stamp.
    /// </summary>
    /// <param name="now">The current UTC instant, stamped as the usage time.</param>
    /// <returns><c>true</c> if the OTP transitioned; <c>false</c> if already used.</returns>
    public bool MarkAsUsed(DateTime now)
    {
        if (IsUsed)
        {
            return false;
        }

        IsUsed = true;
        UsedAt = now;
        return true;
    }

    /// <summary>
    /// Judges a presented code against this OTP, consuming an attempt on a mismatch. Expiry is
    /// checked before the code so an expired OTP never reveals whether the code was right.
    /// </summary>
    /// <param name="suppliedCodeMatches">Whether the presented code matches the stored hash.</param>
    /// <param name="now">The current UTC instant.</param>
    /// <returns>The verification status of the presented code.</returns>
    public EnumOtpVerificationStatus Verify(bool suppliedCodeMatches, DateTime now)
    {
        if (IsExpired(now: now))
        {
            return EnumOtpVerificationStatus.Expired;
        }

        if (HasMaxAttemptsReached())
        {
            return EnumOtpVerificationStatus.AttemptsExhausted;
        }

        if (suppliedCodeMatches)
        {
            return EnumOtpVerificationStatus.Valid;
        }

        AttemptCount++;

        return HasMaxAttemptsReached()
            ? EnumOtpVerificationStatus.AttemptsExhausted
            : EnumOtpVerificationStatus.Mismatch;
    }

    /// <summary>
    /// Checks if the OTP has expired at the supplied instant.
    /// </summary>
    /// <param name="now">The current UTC instant.</param>
    /// <returns>True if the OTP has expired, otherwise false.</returns>
    public bool IsExpired(DateTime now)
    {
        return now > ExpiresAt;
    }

    /// <summary>
    /// Checks if the maximum attempts have been reached.
    /// </summary>
    /// <returns>True if maximum attempts reached, otherwise false.</returns>
    public bool HasMaxAttemptsReached()
    {
        return AttemptCount >= UserConstants.MaxOtpAttempts;
    }

    /// <summary>
    /// Marks the code spent or superseded, so it can never be presented again. Idempotent: a
    /// consumed code keeps its first stamp.
    /// </summary>
    /// <param name="now">The current UTC instant, stamped on the first call only.</param>
    public void MarkAsConsumed(DateTime now)
    {
        ConsumedAt ??= now;
    }
}
