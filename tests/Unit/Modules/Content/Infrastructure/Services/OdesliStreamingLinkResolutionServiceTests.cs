using System.Net;
using System.Text;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Services;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="OdesliStreamingLinkResolutionService"/> against a scripted
/// <see cref="HttpMessageHandler"/> — no real Odesli call is ever made.
/// </summary>
public class OdesliStreamingLinkResolutionServiceTests
{
    private const string SourceUrl = "https://open.spotify.com/album/abc123";

    /// <summary>
    /// Scripted handler capturing the outgoing request and returning a canned response.
    /// </summary>
    private sealed class ScriptedHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            LastRequest = request;
            return Task.FromResult(
                new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                }
            );
        }
    }

    private static OdesliStreamingLinkResolutionService CreateService(ScriptedHandler handler, string? apiKey = null)
    {
        var settings = new Dictionary<string, string?>();
        if (apiKey is not null)
        {
            settings["ODESLI_API_KEY"] = apiKey;
        }

        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new OdesliStreamingLinkResolutionService(new HttpClient(handler), configuration);
    }

    [Fact]
    public async Task ResolveAsync_ShouldMapEveryKnownPlatformKey()
    {
        // Arrange
        const string body = """
            {
              "linksByPlatform": {
                "spotify":      { "url": "https://open.spotify.com/album/1" },
                "appleMusic":   { "url": "https://music.apple.com/album/2" },
                "youtubeMusic": { "url": "https://music.youtube.com/playlist/3" },
                "tidal":        { "url": "https://listen.tidal.com/album/4" },
                "deezer":       { "url": "https://www.deezer.com/album/5" }
              }
            }
            """;
        var handler = new ScriptedHandler(HttpStatusCode.OK, body);

        // Act
        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await CreateService(handler)
            .ResolveAsync(SourceUrl);

        // Assert
        result.Should().HaveCount(5);
        result[EnumStreamingPlatform.Spotify].Should().Be("https://open.spotify.com/album/1");
        result[EnumStreamingPlatform.Deezer].Should().Be("https://www.deezer.com/album/5");
    }

    [Fact]
    public async Task ResolveAsync_ShouldSkipUnknownPlatformKeysAndNonHttpsUrls()
    {
        // Arrange — Odesli adding a platform, or serving a non-https URL, must never break us.
        const string body = """
            {
              "linksByPlatform": {
                "spotify":     { "url": "https://open.spotify.com/album/1" },
                "amazonMusic": { "url": "https://music.amazon.com/albums/9" },
                "tidal":       { "url": "http://listen.tidal.com/album/4" },
                "deezer":      { "url": "javascript:alert(1)" }
              }
            }
            """;
        var handler = new ScriptedHandler(HttpStatusCode.OK, body);

        // Act
        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await CreateService(handler)
            .ResolveAsync(SourceUrl);

        // Assert — only the https Spotify link survives.
        result.Should().HaveCount(1);
        result.Should().ContainKey(EnumStreamingPlatform.Spotify);
    }

    [Fact]
    public async Task ResolveAsync_WithMissingLinksByPlatform_ShouldThrow()
    {
        // Arrange — a body without linksByPlatform is a provider fault, not an empty result.
        var handler = new ScriptedHandler(HttpStatusCode.OK, """{ "entityUniqueId": "x" }""");

        // Act
        Func<Task> act = () => CreateService(handler).ResolveAsync(SourceUrl);

        // Assert
        await act.Should().ThrowAsync<StreamingLinkResolutionException>();
    }

    [Fact]
    public async Task ResolveAsync_WithMalformedJson_ShouldThrow()
    {
        var handler = new ScriptedHandler(HttpStatusCode.OK, "not json at all");

        Func<Task> act = () => CreateService(handler).ResolveAsync(SourceUrl);

        await act.Should().ThrowAsync<StreamingLinkResolutionException>();
    }

    [Fact]
    public async Task ResolveAsync_With429_ShouldThrowRateLimitedFlavour()
    {
        // Arrange — 429 is surfaced distinctly so the admin is told to wait, not retry.
        var handler = new ScriptedHandler(HttpStatusCode.TooManyRequests, "{}");

        // Act
        Func<Task> act = () => CreateService(handler).ResolveAsync(SourceUrl);

        // Assert
        (await act.Should().ThrowAsync<StreamingLinkResolutionException>())
            .Which.IsRateLimited.Should()
            .BeTrue();
    }

    [Fact]
    public async Task ResolveAsync_WithNonSuccessStatus_ShouldThrowNonRateLimitedFlavour()
    {
        var handler = new ScriptedHandler(HttpStatusCode.BadRequest, "{}");

        Func<Task> act = () => CreateService(handler).ResolveAsync(SourceUrl);

        (await act.Should().ThrowAsync<StreamingLinkResolutionException>()).Which.IsRateLimited.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveAsync_ShouldEncodeTheSourceUrlAndPinTheUserCountry()
    {
        // Arrange
        var handler = new ScriptedHandler(HttpStatusCode.OK, """{ "linksByPlatform": {} }""");

        // Act — an empty linksByPlatform is a valid (if useless) response; the request matters here.
        await CreateService(handler).ResolveAsync(SourceUrl);

        // Assert
        string requestUrl = handler.LastRequest!.RequestUri!.ToString();
        requestUrl.Should().Contain("url=" + WebUtility.UrlEncode(SourceUrl));
        requestUrl.Should().Contain("userCountry=CD");
        requestUrl.Should().NotContain("key=");
    }

    [Fact]
    public async Task ResolveAsync_WithConfiguredApiKey_ShouldAppendIt()
    {
        // Arrange
        var handler = new ScriptedHandler(HttpStatusCode.OK, """{ "linksByPlatform": {} }""");

        // Act
        await CreateService(handler, apiKey: "secret-key").ResolveAsync(SourceUrl);

        // Assert
        handler.LastRequest!.RequestUri!.ToString().Should().Contain("key=secret-key");
    }
}
