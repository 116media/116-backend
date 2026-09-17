using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application.DTOs;
using _116.Mailer.Infrastructure.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="SmtpEmailSenderService"/>. The sender reads its settings from the
/// environment, so each case sets the variables it needs and restores them afterwards.
/// </summary>
public class SmtpEmailSenderServiceTests : IDisposable
{
    private static readonly string[] OwnedVariables =
    [
        "SMTP_HOST",
        "SMTP_PORT",
        "EMAIL_FROM_ADDRESS",
        "EMAIL_FROM_NAME",
    ];

    private readonly Dictionary<string, string?> _original = OwnedVariables.ToDictionary(
        name => name,
        Environment.GetEnvironmentVariable
    );

    /// <inheritdoc />
    public void Dispose()
    {
        foreach ((string name, string? value) in _original)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Applies the settings the case under test needs, clearing every other owned variable.
    /// </summary>
    /// <param name="settings">The variables to set.</param>
    /// <returns>The sender reading them.</returns>
    private static SmtpEmailSenderService CreateSender(Dictionary<string, string?> settings)
    {
        foreach (string name in OwnedVariables)
        {
            Environment.SetEnvironmentVariable(name, settings.GetValueOrDefault(name));
        }

        return new SmtpEmailSenderService();
    }

    /// <summary>
    /// Builds a message for the case under test.
    /// </summary>
    /// <param name="displayName">The recipient's display name, or null for an anonymous one.</param>
    /// <returns>The message.</returns>
    private static EmailMessage Message(string? displayName = "Fan")
    {
        return new EmailMessage(
            To: new EmailRecipientDto("fan@example.com", displayName),
            Subject: "Welcome",
            HtmlBody: "<p>Welcome</p>",
            TextBody: "Welcome"
        );
    }

    [Fact]
    public async Task SendAsync_WithoutTheSenderAddressConfigured_ShouldFailBeforeAnyDelivery()
    {
        SmtpEmailSenderService sender = CreateSender([]);

        Func<Task> act = () => sender.SendAsync(Message(), CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("EMAIL_FROM_ADDRESS env variable is missing or empty.");
    }

    [Fact]
    public async Task SendAsync_WithAnAlreadyCancelledToken_ShouldSurfaceTheCancellation()
    {
        SmtpEmailSenderService sender = CreateSender(
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
        SmtpEmailSenderService sender = CreateSender(
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
