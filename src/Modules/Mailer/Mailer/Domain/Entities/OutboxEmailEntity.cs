using System.ComponentModel.DataAnnotations;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Mailer.Domain.Entities;

/// <summary>
/// A fully rendered email awaiting reliable background delivery.
/// <para>
/// The row is self-contained: subject and both bodies are rendered at enqueue
/// time, so it stays replayable even if the template that produced it changes
/// later. The dispatcher scans <c>(status, next_attempt_at)</c>, delivers, and
/// records the outcome through <see cref="MarkSent" /> or
/// <see cref="RegisterFailure" />.
/// </para>
/// </summary>
public class OutboxEmailEntity : Aggregate<Guid>
{
    /// <summary>
    /// The recipient email address.
    /// </summary>
    [MaxLength(length: 320)]
    public string RecipientAddress { get; private set; } = null!;

    /// <summary>
    /// The optional recipient display name shown by mail clients.
    /// </summary>
    [MaxLength(length: 200)]
    public string? RecipientName { get; private set; }

    /// <summary>
    /// The rendered subject line.
    /// </summary>
    [MaxLength(length: 500)]
    public string Subject { get; private set; } = null!;

    /// <summary>
    /// The rendered HTML body.
    /// </summary>
    public string HtmlBody { get; private set; } = null!;

    /// <summary>
    /// The rendered plain-text body.
    /// </summary>
    public string TextBody { get; private set; } = null!;

    /// <summary>
    /// The template that produced this email, stored for observability and
    /// filtering. Persisted as a string so template renames never corrupt rows.
    /// </summary>
    [MaxLength(length: 100)]
    public string Template { get; private set; } = null!;

    /// <summary>
    /// The delivery lifecycle state of this email.
    /// </summary>
    public EnumOutboxEmailStatus Status { get; private set; }

    /// <summary>
    /// Number of delivery attempts performed so far.
    /// </summary>
    public int AttemptCount { get; private set; }

    /// <summary>
    /// The earliest moment the dispatcher may (re)attempt delivery.
    /// </summary>
    public DateTime NextAttemptAt { get; private set; }

    /// <summary>
    /// The truncated provider error from the most recent failed attempt.
    /// </summary>
    [MaxLength(length: MailerConstants.MaxLastErrorLength)]
    public string? LastError { get; private set; }

    /// <summary>
    /// When the email was successfully handed to the provider.
    /// </summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private OutboxEmailEntity() { }

    /// <summary>
    /// Creates a pending outbox email ready for its first delivery attempt.
    /// </summary>
    /// <param name="id">The unique identifier for the outbox email.</param>
    /// <param name="recipientAddress">The recipient email address.</param>
    /// <param name="recipientName">The optional recipient display name.</param>
    /// <param name="subject">The rendered subject line.</param>
    /// <param name="htmlBody">The rendered HTML body.</param>
    /// <param name="textBody">The rendered plain-text body.</param>
    /// <param name="template">The catalog name of the template that produced it.</param>
    /// <param name="now">The current UTC time, used as the first attempt time.</param>
    /// <returns>A new pending <see cref="OutboxEmailEntity" />.</returns>
    public static OutboxEmailEntity Enqueue(
        Guid id,
        string recipientAddress,
        string? recipientName,
        string subject,
        string htmlBody,
        string textBody,
        string template,
        DateTime now
    )
    {
        return new OutboxEmailEntity
        {
            Id = id,
            RecipientAddress = recipientAddress,
            RecipientName = recipientName,
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            Template = template,
            Status = EnumOutboxEmailStatus.Pending,
            AttemptCount = 0,
            NextAttemptAt = now,
        };
    }

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
    }

    /// <summary>
    /// Records a failed delivery attempt. Transient failures schedule the next
    /// attempt from the backoff schedule until it is exhausted; permanent
    /// failures (and exhaustion) mark the email failed.
    /// </summary>
    /// <param name="error">The provider error, truncated for storage.</param>
    /// <param name="isTransient">Whether the failure is worth retrying.</param>
    /// <param name="now">The current UTC time.</param>
    public void RegisterFailure(string error, bool isTransient, DateTime now)
    {
        AttemptCount++;
        LastError =
            error.Length > MailerConstants.MaxLastErrorLength ? error[..MailerConstants.MaxLastErrorLength] : error;

        if (!isTransient || AttemptCount >= MailerConstants.MaxAttempts)
        {
            Status = EnumOutboxEmailStatus.Failed;
            return;
        }

        NextAttemptAt = now + MailerConstants.RetrySchedule[AttemptCount - 1];
    }
}
