using System.Net;
using System.Text;
using _116.Content.Application.Shared.Exceptions;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Services;

/// <summary>
/// Integration tests for <see cref="OdesliStreamingLinkResolutionService"/> against a real
/// loopback HTTP server — real sockets, real <see cref="HttpClient"/>, real JSON parsing.
/// The API host stubs this adapter out (like Cloudinary), so this is the one place its real
/// code path executes end to end; owning the server keeps it deterministic where calling
/// the live Odesli API could not be.
/// </summary>
public class OdesliStreamingLinkResolutionServiceTests
{
    private const string SourceUrl = "https://open.spotify.com/album/abc123";

    /// <summary>
    /// Minimal one-request loopback server: serves the scripted status and body on a random
    /// free port, capturing the request path and query for assertions.
    /// </summary>
    private sealed class LoopbackServer : IDisposable
    {
        private readonly HttpListener _listener = new();
        private readonly Task _serving;

        public string BaseUrl { get; }
        public string? LastRawUrl { get; private set; }

        public LoopbackServer(HttpStatusCode statusCode, string body)
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

                    LastRawUrl = context.Request.RawUrl;
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

    private static OdesliStreamingLinkResolutionService CreateService(string baseUrl, string? apiKey = null)
    {
        var settings = new Dictionary<string, string?> { ["ODESLI_API_URL"] = baseUrl };
        if (apiKey is not null)
        {
            settings["ODESLI_API_KEY"] = apiKey;
        }

        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new OdesliStreamingLinkResolutionService(new HttpClient(), configuration);
    }

    [Fact]
    public async Task ResolveAsync_OverRealHttp_MapsEveryModelledPlatform()
    {
        const string body = """
            {
              "linksByPlatform": {
                "spotify":      { "url": "https://open.spotify.com/album/1" },
                "appleMusic":   { "url": "https://music.apple.com/album/2" },
                "youtubeMusic": { "url": "https://music.youtube.com/playlist/3" },
                "tidal":        { "url": "https://listen.tidal.com/album/4" },
                "deezer":       { "url": "https://www.deezer.com/album/5" },
                "amazonMusic":  { "url": "https://music.amazon.com/albums/9" }
              }
            }
            """;
        using var server = new LoopbackServer(HttpStatusCode.OK, body);

        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await CreateService(server.BaseUrl)
            .ResolveAsync(SourceUrl);

        result.Should().HaveCount(5);
        result[EnumStreamingPlatform.Deezer].Should().Be("https://www.deezer.com/album/5");

        // The request that actually crossed the wire carries the contract details.
        server.LastRawUrl.Should().Contain("url=" + WebUtility.UrlEncode(SourceUrl));
        server.LastRawUrl.Should().Contain("userCountry=CD");
        server.LastRawUrl.Should().NotContain("key=");
    }

    [Fact]
    public async Task ResolveAsync_WithConfiguredKey_SendsItOnTheWire()
    {
        using var server = new LoopbackServer(HttpStatusCode.OK, """{ "linksByPlatform": {} }""");

        await CreateService(server.BaseUrl, apiKey: "raise-the-limit").ResolveAsync(SourceUrl);

        server.LastRawUrl.Should().Contain("key=raise-the-limit");
    }

    [Fact]
    public async Task ResolveAsync_WhenServerRateLimits_ThrowsRateLimitedFlavour()
    {
        using var server = new LoopbackServer(HttpStatusCode.TooManyRequests, "{}");

        Func<Task> act = () => CreateService(server.BaseUrl).ResolveAsync(SourceUrl);

        (await act.Should().ThrowAsync<StreamingLinkResolutionException>()).Which.IsRateLimited.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveAsync_WhenServerErrors_ThrowsNonRateLimitedFlavour()
    {
        using var server = new LoopbackServer(HttpStatusCode.InternalServerError, "{}");

        Func<Task> act = () => CreateService(server.BaseUrl).ResolveAsync(SourceUrl);

        (await act.Should().ThrowAsync<StreamingLinkResolutionException>()).Which.IsRateLimited.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveAsync_WhenBodyCarriesNoLinksByPlatform_Throws()
    {
        // Valid JSON, wrong shape — a provider fault, not an empty result.
        using var server = new LoopbackServer(HttpStatusCode.OK, """{ "entityUniqueId": "x" }""");

        Func<Task> act = () => CreateService(server.BaseUrl).ResolveAsync(SourceUrl);

        (await act.Should().ThrowAsync<StreamingLinkResolutionException>()).Which.IsRateLimited.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveAsync_SkipsEntriesWithoutAValidHttpsUrl()
    {
        // Missing url, null url and a non-https scheme are all dropped; the one sound
        // entry still resolves.
        const string body = """
            {
              "linksByPlatform": {
                "spotify":      { "nativeAppUriDesktop": "spotify:album:1" },
                "appleMusic":   { "url": null },
                "youtubeMusic": { "url": "http://music.youtube.com/playlist/3" },
                "tidal":        { "url": "https://listen.tidal.com/album/4" }
              }
            }
            """;
        using var server = new LoopbackServer(HttpStatusCode.OK, body);

        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await CreateService(server.BaseUrl)
            .ResolveAsync(SourceUrl);

        result.Should().HaveCount(1);
        result[EnumStreamingPlatform.Tidal].Should().Be("https://listen.tidal.com/album/4");
    }

    [Fact]
    public async Task ResolveAsync_WhenServerReturnsMalformedJson_Throws()
    {
        using var server = new LoopbackServer(HttpStatusCode.OK, "not json");

        Func<Task> act = () => CreateService(server.BaseUrl).ResolveAsync(SourceUrl);

        await act.Should().ThrowAsync<StreamingLinkResolutionException>();
    }

    [Fact]
    public async Task ResolveAsync_WhenNothingListens_ThrowsUnreachable()
    {
        // A port nothing listens on — the connection itself fails.
        Func<Task> act = () => CreateService("http://127.0.0.1:59999").ResolveAsync(SourceUrl);

        await act.Should().ThrowAsync<StreamingLinkResolutionException>();
    }
}
