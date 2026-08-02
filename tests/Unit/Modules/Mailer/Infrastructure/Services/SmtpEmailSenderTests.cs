using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Infrastructure.Services;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="SmtpEmailSender" /> covering the paths that are
/// decided before a connection is attempted: the sender identity the MIME
/// build requires, and cancellation, which must surface as a cancellation
/// rather than a delivery failure.
/// </summary>
public class SmtpEmailSenderTests
{
    private static SmtpEmailSender CreateSender(Dictionary<string, string?> settings)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new SmtpEmailSender(configuration);
    }

    private static EmailMessage Message(string? displayName = "Fan")
    {
        return new EmailMessage(
            To: new EmailRecipient("fan@example.com", displayName),
            Subject: "Welcome",
            HtmlBody: "<p>Welcome</p>",
            TextBody: "Welcome"
        );
    }

    [Fact]
    public async Task SendAsync_WithoutTheSenderAddressConfigured_ShouldFailBeforeAnyDelivery()
    {
        SmtpEmailSender sender = CreateSender([]);

        Func<Task> act = () => sender.SendAsync(Message(), CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("EMAIL_FROM_ADDRESS env variable is missing or empty.");
    }

    [Fact]
    public async Task SendAsync_WithAnAlreadyCancelledToken_ShouldSurfaceTheCancellation()
    {
        SmtpEmailSender sender = CreateSender(
            new Dictionary<string, string?>
            {
                ["SMTP_HOST"] = "127.0.0.1",
                ["SMTP_PORT"] = "1025",
                ["EMAIL_FROM_ADDRESS"] = "no-reply@116.example",
            }
        );

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        Func<Task> act = () => sender.SendAsync(Message(), cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SendAsync_WithAConfiguredSenderNameAndAnAnonymousRecipient_ShouldStillBuildTheMessage()
    {
        // A recipient without a display name and an explicit sender name are the
        // other side of both identity fallbacks; the send is cancelled so the
        // message is built without a connection ever being attempted.
        SmtpEmailSender sender = CreateSender(
            new Dictionary<string, string?>
            {
                ["SMTP_HOST"] = "127.0.0.1",
                ["SMTP_PORT"] = "1025",
                ["EMAIL_FROM_ADDRESS"] = "no-reply@116.example",
                ["EMAIL_FROM_NAME"] = "116 Newsroom",
            }
        );

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        Func<Task> act = () => sender.SendAsync(Message(displayName: null), cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
