using System.Text;
using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace _116.Integration.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Integration tests for <see cref="ResendEmailSender" /> against a real
/// loopback HTTP server — real sockets, real <see cref="HttpClient" /> — the
/// same pattern as the Odesli adapter tests.
/// </summary>
public class ResendEmailSenderTests
{
    /// <summary>
    /// Minimal one-request loopback server: serves the scripted status and
    /// captures the request body and authorization header.
    /// </summary>
    private sealed class LoopbackServer : IDisposable
    {
        private readonly HttpListener _listener = new();
        private readonly Task _serving;

        public string BaseUrl { get; }
        public string? LastBody { get; private set; }
        public string? LastAuthorization { get; private set; }

        public LoopbackServer(HttpStatusCode statusCode, string body = "{}")
        {
            int port = FreePort();
            BaseUrl = $"http://127.0.0.1:{port}";
            _listener.Prefixes.Add($"{BaseUrl}/");
            _listener.Start();
            _serving = Task.Run(async () =>
            {
                while (_listener.IsListening)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = await _listener.GetContextAsync();
                    }
                    catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException)
                    {
                        return;
                    }

                    using var reader = new StreamReader(context.Request.InputStream);
                    LastBody = await reader.ReadToEndAsync();
                    LastAuthorization = context.Request.Headers["Authorization"];

                    byte[] payload = Encoding.UTF8.GetBytes(body);
                    context.Response.StatusCode = (int)statusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.OutputStream.WriteAsync(payload);
                    context.Response.Close();
                }
            });
        }

        private static int FreePort()
        {
            var socket = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            socket.Start();
            int port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            return port;
        }

        public void Dispose()
        {
            _listener.Stop();
            _listener.Close();
            _serving.Wait(TimeSpan.FromSeconds(2));
        }
    }

    private static ResendEmailSender CreateSender(string baseUrl)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RESEND_API_URL"] = baseUrl,
                    ["RESEND_API_KEY"] = "re_test_key",
                    ["EMAIL_FROM_ADDRESS"] = "no-reply@116.example",
                    ["EMAIL_FROM_NAME"] = "116",
                }
            )
            .Build();

        return new ResendEmailSender(new HttpClient(), configuration);
    }

    private static EmailMessage Message()
    {
        return new EmailMessage(
            To: new EmailRecipient("fan@example.com", "Fan"),
            Subject: "Wire subject",
            HtmlBody: "<p>Hi</p>",
            TextBody: "Hi"
        );
    }

    [Fact]
    public async Task SendAsync_OverRealHttp_PostsTheProviderContract()
    {
        using var server = new LoopbackServer(HttpStatusCode.OK, """{ "id": "email_1" }""");

        await CreateSender(server.BaseUrl).SendAsync(Message(), CancellationToken.None);

        server.LastAuthorization.Should().Be("Bearer re_test_key");
        server.LastBody.Should().Contain("fan@example.com");
        server.LastBody.Should().Contain("Wire subject");
        server.LastBody.Should().Contain("no-reply@116.example");
    }

    [Fact]
    public async Task SendAsync_WhenRateLimited_ThrowsTransient()
    {
        using var server = new LoopbackServer(HttpStatusCode.TooManyRequests);

        Func<Task> act = () => CreateSender(server.BaseUrl).SendAsync(Message(), CancellationToken.None);

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.IsTransient.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WhenPayloadRejected_ThrowsPermanent()
    {
        using var server = new LoopbackServer(HttpStatusCode.UnprocessableEntity);

        Func<Task> act = () => CreateSender(server.BaseUrl).SendAsync(Message(), CancellationToken.None);

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.IsTransient.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenServerErrors_ThrowsTransient()
    {
        using var server = new LoopbackServer(HttpStatusCode.InternalServerError);

        Func<Task> act = () => CreateSender(server.BaseUrl).SendAsync(Message(), CancellationToken.None);

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.IsTransient.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WhenNothingListens_ThrowsTransient()
    {
        Func<Task> act = () => CreateSender("http://127.0.0.1:59997").SendAsync(Message(), CancellationToken.None);

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.IsTransient.Should().BeTrue();
    }
}
