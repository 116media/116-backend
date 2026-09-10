using System.Net.Sockets;
using System.Text;
using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application.DTOs;
using _116.Mailer.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace _116.Integration.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Integration tests for <see cref="SmtpEmailSenderService" /> against a real loopback
/// SMTP session — real sockets, real MailKit protocol exchange. The API host
/// stubs the sender, so this is the one place the real adapter executes end to
/// end; owning the server keeps it deterministic.
/// </summary>
public class SmtpEmailSenderServiceTests : IDisposable
{
    /// <summary>
    /// The variables this class sets, restored after each case so the suite stays order-free.
    /// </summary>
    private static readonly string[] OwnedVariables =
    [
        "SMTP_HOST",
        "SMTP_PORT",
        "SMTP_USERNAME",
        "SMTP_PASSWORD",
        "EMAIL_FROM_ADDRESS",
        "EMAIL_FROM_NAME",
    ];

    private readonly Dictionary<string, string?> _originalEnvironment = OwnedVariables.ToDictionary(
        name => name,
        Environment.GetEnvironmentVariable
    );

    /// <inheritdoc />
    public void Dispose()
    {
        foreach ((string name, string? value) in _originalEnvironment)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Minimal one-session SMTP server: speaks just enough of the protocol for
    /// one delivery, captures the DATA payload, and can be told to refuse the
    /// recipient with a permanent 554 or to advertise AUTH.
    /// </summary>
    private sealed class LoopbackSmtpServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly Task _serving;

        public int Port { get; }
        public string Data { get; private set; } = string.Empty;

        /// <summary>
        /// The credentials the client presented, or null when it never authenticated.
        /// </summary>
        public string? AuthenticatedAs { get; private set; }

        public LoopbackSmtpServer(bool rejectRecipient = false, bool offerAuth = false)
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _serving = Task.Run(async () =>
            {
                using TcpClient client = await _listener.AcceptTcpClientAsync();
                using NetworkStream stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII);
                using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true, NewLine = "\r\n" };

                await writer.WriteLineAsync("220 loopback ready");
                bool inData = false;
                var data = new StringBuilder();

                while (await reader.ReadLineAsync() is { } line)
                {
                    if (inData)
                    {
                        if (line == ".")
                        {
                            inData = false;
                            Data = data.ToString();
                            await writer.WriteLineAsync("250 OK stored");
                            continue;
                        }

                        data.AppendLine(line);
                        continue;
                    }

                    string verb = line.Split(' ', ':')[0].ToUpperInvariant();
                    switch (verb)
                    {
                        case "EHLO":
                        case "HELO":
                            await writer.WriteLineAsync("250-loopback");
                            if (offerAuth)
                            {
                                await writer.WriteLineAsync("250-AUTH PLAIN LOGIN");
                            }

                            await writer.WriteLineAsync("250 OK");
                            break;
                        case "AUTH":
                            AuthenticatedAs = DecodePlainCredentials(line);
                            await writer.WriteLineAsync("235 2.7.0 authenticated");
                            break;
                        case "MAIL":
                            await writer.WriteLineAsync("250 OK");
                            break;
                        case "RCPT":
                            await writer.WriteLineAsync(rejectRecipient ? "554 5.7.1 rejected" : "250 OK");
                            break;
                        case "DATA":
                            inData = true;
                            await writer.WriteLineAsync("354 go ahead");
                            break;
                        case "QUIT":
                            await writer.WriteLineAsync("221 bye");
                            return;
                        default:
                            await writer.WriteLineAsync("250 OK");
                            break;
                    }
                }
            });
        }

        /// <summary>
        /// Reads the username out of an <c>AUTH PLAIN</c> command, whose payload decodes to
        /// authorization id, username and password separated by NUL.
        /// </summary>
        /// <param name="line">The command line as received.</param>
        /// <returns>The username, or null when the command carried no initial response.</returns>
        private static string? DecodePlainCredentials(string line)
        {
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
            {
                return null;
            }

            string[] fields = Encoding.UTF8.GetString(Convert.FromBase64String(parts[2])).Split('\0');

            return fields.Length >= 2 ? fields[1] : null;
        }

        public void Dispose()
        {
            _listener.Stop();
            _serving.Wait(TimeSpan.FromSeconds(2));
        }
    }

    private static SmtpEmailSenderService CreateSender(int port, string? username = null, string? password = null)
    {
        Environment.SetEnvironmentVariable("SMTP_HOST", "127.0.0.1");
        Environment.SetEnvironmentVariable("SMTP_PORT", port.ToString());
        Environment.SetEnvironmentVariable("EMAIL_FROM_ADDRESS", "no-reply@116.example");
        Environment.SetEnvironmentVariable("EMAIL_FROM_NAME", "116");
        Environment.SetEnvironmentVariable("SMTP_USERNAME", username);
        Environment.SetEnvironmentVariable("SMTP_PASSWORD", password);

        return new SmtpEmailSenderService();
    }

    private static EmailMessage Message()
    {
        return new EmailMessage(
            To: new EmailRecipientDto("fan@example.com", "Fan"),
            Subject: "Loopback subject",
            HtmlBody: "<p>Hello over the wire</p>",
            TextBody: "Hello over the wire"
        );
    }

    [Fact]
    public async Task SendAsync_OverRealSmtp_DeliversBothBodyPartsAndSenderIdentity()
    {
        using var server = new LoopbackSmtpServer();

        await CreateSender(server.Port).SendAsync(Message(), CancellationToken.None);

        server.Data.Should().Contain("Loopback subject");
        server.Data.Should().Contain("Hello over the wire");
        server.Data.Should().Contain("text/plain").And.Contain("text/html");
        server.Data.Should().Contain("no-reply@116.example");
    }

    [Fact]
    public async Task SendAsync_WithCredentialsConfigured_AuthenticatesBeforeDelivering()
    {
        using var server = new LoopbackSmtpServer(offerAuth: true);

        await CreateSender(server.Port, username: "mailer@116.example", password: "s3cret")
            .SendAsync(Message(), CancellationToken.None);

        server.AuthenticatedAs.Should().Be("mailer@116.example");
        server.Data.Should().Contain("Loopback subject");
    }

    [Fact]
    public async Task SendAsync_WhenServerRejectsRecipient_ThrowsPermanent()
    {
        using var server = new LoopbackSmtpServer(rejectRecipient: true);

        Func<Task> act = () => CreateSender(server.Port).SendAsync(Message(), CancellationToken.None);

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.IsTransient.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenNothingListens_ThrowsTransient()
    {
        Func<Task> act = () => CreateSender(port: 59998).SendAsync(Message(), CancellationToken.None);

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.IsTransient.Should().BeTrue();
    }
}
