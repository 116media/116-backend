using System.Net.Sockets;
using System.Text;
using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace _116.Integration.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Integration tests for <see cref="SmtpEmailSender" /> against a real loopback
/// SMTP session — real sockets, real MailKit protocol exchange. The API host
/// stubs the sender, so this is the one place the real adapter executes end to
/// end; owning the server keeps it deterministic.
/// </summary>
public class SmtpEmailSenderTests
{
    /// <summary>
    /// Minimal one-session SMTP server: speaks just enough of the protocol for
    /// one delivery, captures the DATA payload, and can be told to refuse the
    /// recipient with a permanent 554.
    /// </summary>
    private sealed class LoopbackSmtpServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly Task _serving;

        public int Port { get; }
        public string Data { get; private set; } = string.Empty;

        public LoopbackSmtpServer(bool rejectRecipient = false)
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
                            await writer.WriteLineAsync("250 OK");
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

        public void Dispose()
        {
            _listener.Stop();
            _serving.Wait(TimeSpan.FromSeconds(2));
        }
    }

    private static SmtpEmailSender CreateSender(int port)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["SMTP_HOST"] = "127.0.0.1",
                    ["SMTP_PORT"] = port.ToString(),
                    ["EMAIL_FROM_ADDRESS"] = "no-reply@116.example",
                    ["EMAIL_FROM_NAME"] = "116",
                }
            )
            .Build();

        return new SmtpEmailSender(configuration);
    }

    private static EmailMessage Message()
    {
        return new EmailMessage(
            To: new EmailRecipient("fan@example.com", "Fan"),
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
