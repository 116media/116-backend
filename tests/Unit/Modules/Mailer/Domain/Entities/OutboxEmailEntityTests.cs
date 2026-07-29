using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="OutboxEmailEntity" /> covering the enqueue state
/// and every delivery-outcome transition.
/// </summary>
public class OutboxEmailEntityTests
{
    private static readonly DateTime Now = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static OutboxEmailEntity Enqueue()
    {
        return OutboxEmailEntity.Enqueue(
            id: Guid.NewGuid(),
            recipientAddress: "fan@example.com",
            recipientName: "Fan",
            subject: "Subject",
            htmlBody: "<p>Hi</p>",
            textBody: "Hi",
            template: "Welcome",
            now: Now
        );
    }

    [Fact]
    public void Enqueue_ShouldStartPendingAndDueImmediately()
    {
        OutboxEmailEntity email = Enqueue();

        email.Status.Should().Be(EnumOutboxEmailStatus.Pending);
        email.AttemptCount.Should().Be(0);
        email.NextAttemptAt.Should().Be(Now);
        email.SentAt.Should().BeNull();
        email.LastError.Should().BeNull();
    }

    [Fact]
    public void MarkSent_ShouldRecordTimeAndClearError()
    {
        OutboxEmailEntity email = Enqueue();
        email.RegisterFailure(error: "boom", isTransient: true, now: Now);

        email.MarkSent(now: Now.AddMinutes(2));

        email.Status.Should().Be(EnumOutboxEmailStatus.Sent);
        email.SentAt.Should().Be(Now.AddMinutes(2));
        email.LastError.Should().BeNull();
    }

    [Fact]
    public void MarkSent_Twice_ShouldKeepTheFirstTimestamp()
    {
        OutboxEmailEntity email = Enqueue();
        email.MarkSent(now: Now);

        email.MarkSent(now: Now.AddHours(1));

        email.SentAt.Should().Be(Now);
    }

    [Fact]
    public void RegisterFailure_Transient_ShouldFollowTheBackoffSchedule()
    {
        OutboxEmailEntity email = Enqueue();

        for (int attempt = 1; attempt < MailerConstants.MaxAttempts; attempt++)
        {
            email.RegisterFailure(error: $"fail {attempt}", isTransient: true, now: Now);

            email.Status.Should().Be(EnumOutboxEmailStatus.Pending);
            email.AttemptCount.Should().Be(attempt);
            email.NextAttemptAt.Should().Be(Now + MailerConstants.RetrySchedule[attempt - 1]);
        }
    }

    [Fact]
    public void RegisterFailure_ExhaustedSchedule_ShouldFail()
    {
        OutboxEmailEntity email = Enqueue();

        for (int attempt = 1; attempt <= MailerConstants.MaxAttempts; attempt++)
        {
            email.RegisterFailure(error: "still down", isTransient: true, now: Now);
        }

        email.Status.Should().Be(EnumOutboxEmailStatus.Failed);
        email.AttemptCount.Should().Be(MailerConstants.MaxAttempts);
    }

    [Fact]
    public void RegisterFailure_Permanent_ShouldFailImmediately()
    {
        OutboxEmailEntity email = Enqueue();

        email.RegisterFailure(error: "mailbox does not exist", isTransient: false, now: Now);

        email.Status.Should().Be(EnumOutboxEmailStatus.Failed);
        email.AttemptCount.Should().Be(1);
        email.LastError.Should().Be("mailbox does not exist");
    }

    [Fact]
    public void RegisterFailure_ShouldTruncateOverlongErrors()
    {
        OutboxEmailEntity email = Enqueue();
        string longError = new('x', MailerConstants.MaxLastErrorLength + 50);

        email.RegisterFailure(error: longError, isTransient: true, now: Now);

        email.LastError.Should().HaveLength(MailerConstants.MaxLastErrorLength);
    }
}
