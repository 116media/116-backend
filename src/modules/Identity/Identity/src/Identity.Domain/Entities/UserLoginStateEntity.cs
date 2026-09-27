using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Per-user login brute-force record, kept 1:1 with the user (the <see cref="Entity{T}.Id" /> is
/// the user id). The counters live here rather than on <see cref="UserEntity" /> because they
/// move by atomic SQL, never by a tracked mutation, so an increment survives the exception the
/// failed attempt throws and never contends on the user's own row.
/// </summary>
public class UserLoginStateEntity : Aggregate<Guid>
{
    /// <summary>
    /// Consecutive failed login attempts since the last success.
    /// </summary>
    public int FailedAttempts { get; private set; }

    /// <summary>
    /// UTC instant until which login is refused, or null when the account is not locked.
    /// </summary>
    public DateTime? LockedUntil { get; private set; }

    private UserLoginStateEntity() { }

    /// <summary>
    /// Creates the lockout record for a user; call in the same unit of work as the user.
    /// </summary>
    /// <param name="userId">The user the record belongs to.</param>
    /// <returns>The new record with no failures recorded.</returns>
    public static UserLoginStateEntity Create(Guid userId)
    {
        return new UserLoginStateEntity
        {
            Id = userId,
            FailedAttempts = 0,
            LockedUntil = null,
        };
    }
}
