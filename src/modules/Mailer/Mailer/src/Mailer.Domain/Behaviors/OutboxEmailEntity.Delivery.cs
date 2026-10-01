using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Enums;

namespace _116.Mailer.Domain.Entities;

/// <summary>
/// Delivery transitions of <see cref="OutboxEmailEntity" />. Its state lives in <c>Entities/OutboxEmailEntity.cs</c>.
/// </summary>
public partial class OutboxEmailEntity
{
    /// <summary>
    /// Records a successful hand-off to the provider. Idempotent: marking a
    /// sent email sent again is a no-op.
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    public void MarkSent(DateTime now)
    {
        if (Status == EnumOutboxEmailStatus.Sent)
        {
            return;
        }

        Status = EnumOutboxEmailStatus.Sent;
        SentAt = now;
        LastError = null;
        LeaseExpiresAt = null;
    }

    /// <summary>
    /// Records a failed delivery attempt and releases the dispatcher's claim.
    /// Transient failures schedule the next attempt from the backoff schedule
    /// until it is exhausted; permanent failures (and exhaustion) mark the
    /// email failed.
    /// </summary>
    /// <param name="error">The provider error, truncated for storage.</param>
    /// <param name="isTransient">Whether the failure is worth retrying.</param>
    /// <param name="now">The current UTC time.</param>
    public void RegisterFailure(string error, bool isTransient, DateTime now)
    {
        AttemptCount++;
        LastError =
            error.Length > MailerConstants.MaxLastErrorLength ? error[..MailerConstants.MaxLastErrorLength] : error;
        LeaseExpiresAt = null;

        if (!isTransient || AttemptCount >= MailerConstants.MaxAttempts)
        {
            Status = EnumOutboxEmailStatus.Failed;
            return;
        }

        Status = EnumOutboxEmailStatus.Pending;
        NextAttemptAt = now + MailerConstants.RetrySchedule[AttemptCount - 1];
    }
}
