using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Verification behaviour of <see cref="OtpEntity" />. Its state lives in <c>Entities/OtpEntity.cs</c>.
/// </summary>
public partial class OtpEntity
{
    /// <summary>
    /// Records that the code still owes the user a delivery, carrying the plaintext the row
    /// itself never stores. Raised before the save so the code is delivered only if the flow
    /// that issued it commits.
    /// </summary>
    /// <param name="plainCode">The code to deliver.</param>
    public void MarkIssued(string plainCode)
    {
        AddDomainEvent(new OtpIssuedEvent(UserId: UserId, PlainCode: plainCode, Purpose: Purpose));
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
