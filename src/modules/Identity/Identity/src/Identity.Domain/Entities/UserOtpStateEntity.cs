using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Per-user OTP throttling record, kept 1:1 with the user (the <see cref="Entity{T}.Id" /> is the
/// user id). The counters live here rather than on <see cref="OtpEntity" /> because a resend
/// replaces the OTP row, which would reset an allowance stored on it.
/// </summary>
public class UserOtpStateEntity : Aggregate<Guid>
{
    /// <summary>
    /// Consecutive failed OTP attempts, counted across resends.
    /// </summary>
    public int FailedAttempts { get; private set; }

    /// <summary>
    /// UTC instant until which OTP verification is refused, or null when it is not locked.
    /// </summary>
    public DateTime? LockedUntil { get; private set; }

    private UserOtpStateEntity() { }

    /// <summary>
    /// Creates the throttling record for a user; call in the same unit of work as the user.
    /// </summary>
    /// <param name="userId">The user the record belongs to.</param>
    /// <returns>The new record with no failures recorded.</returns>
    public static UserOtpStateEntity Create(Guid userId)
    {
        return new UserOtpStateEntity
        {
            Id = userId,
            FailedAttempts = 0,
            LockedUntil = null,
        };
    }
}
