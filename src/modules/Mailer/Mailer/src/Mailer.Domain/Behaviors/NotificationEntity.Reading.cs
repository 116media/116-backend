namespace _116.Mailer.Domain.Entities;

/// <summary>
/// Read tracking of <see cref="NotificationEntity" />. Its state lives in <c>Entities/NotificationEntity.cs</c>.
/// </summary>
public partial class NotificationEntity
{
    /// <summary>
    /// Marks the notification read. Idempotent: marking an already read
    /// notification is a no-op that keeps the original read time.
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <returns>True when the notification transitioned to read; false when it already was.</returns>
    public bool MarkRead(DateTime now)
    {
        if (ReadAt is not null)
        {
            return false;
        }

        ReadAt = now;
        return true;
    }
}
