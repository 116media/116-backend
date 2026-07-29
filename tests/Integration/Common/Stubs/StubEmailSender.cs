using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Services;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Records every message the outbox dispatcher hands it instead of contacting
/// a provider. Integration tests assert on the outbox rows in Postgres and,
/// for dispatcher tests, on <see cref="Sent" />. Setting
/// <see cref="NextFailure" /> makes the next send throw once, driving the
/// retry path without any provider.
/// </summary>
public class StubEmailSender : IEmailSender
{
    /// <summary>
    /// Every message handed to the sender, in delivery order.
    /// </summary>
    public List<EmailMessage> Sent { get; } = [];

    /// <summary>
    /// When set, the next <see cref="SendAsync" /> call throws this exception
    /// once and clears the field.
    /// </summary>
    public EmailDeliveryException? NextFailure { get; set; }

    /// <inheritdoc />
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (NextFailure is not null)
        {
            EmailDeliveryException failure = NextFailure;
            NextFailure = null;
            throw failure;
        }

        Sent.Add(message);
        return Task.CompletedTask;
    }
}
