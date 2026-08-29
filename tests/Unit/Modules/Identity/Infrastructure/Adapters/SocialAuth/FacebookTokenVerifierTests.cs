using System.Net;
using System.Text;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Infrastructure.Adapters.SocialAuth;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Unit tests for <see cref="FacebookTokenVerifier"/>. The Graph <c>debug_token</c> and profile calls
/// are served by a stub handler so no real network call is made. A token whose <c>is_valid</c> is
/// false, whose <c>app_id</c> is not ours, or that yields no profile throws
/// <see cref="SocialTokenVerificationException"/>.
/// </summary>
public class FacebookTokenVerifierTests
{
    private const string AppId = "app-id";
    private const string AppSecret = "app-secret";

    private static FacebookTokenVerifier Verifier(string debugTokenJson, string profileJson)
    {
        var handler = new StubHandler(debugTokenJson, profileJson);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(SocialAuthConstants.FacebookGraphBaseUrl) };
        IOptions<SocialAuthOptions> options = Options.Create(
            new SocialAuthOptions { FacebookAppId = AppId, FacebookAppSecret = AppSecret }
        );

        return new FacebookTokenVerifier(httpClient, options);
    }

    private static string ValidDebugToken() => """{"data":{"is_valid":true,"app_id":"app-id"}}""";

    [Fact]
    public async Task VerifyAsync_WithValidTokenAndProfile_ReturnsPayload()
    {
        // Arrange
        string profile =
            """{"id":"fb-123","name":"Test User","email":"user@test.com","picture":{"data":{"url":"https://img/x.png"}}}""";
        FacebookTokenVerifier verifier = Verifier(ValidDebugToken(), profile);

        // Act
        SocialTokenPayload payload = await verifier.VerifyAsync("token", CancellationToken.None);

        // Assert
        payload.ProviderSubjectId.Should().Be("fb-123");
        payload.Email.Should().Be("user@test.com");
        payload.EmailVerified.Should().BeTrue();
        payload.Name.Should().Be("Test User");
        payload.PictureUrl.Should().Be("https://img/x.png");
    }

    [Fact]
    public async Task VerifyAsync_WithoutEmail_TreatsEmailAsUnverified()
    {
        // Arrange
        string profile = """{"id":"fb-123","name":"Test User"}""";
        FacebookTokenVerifier verifier = Verifier(ValidDebugToken(), profile);

        // Act
        SocialTokenPayload payload = await verifier.VerifyAsync("token", CancellationToken.None);

        // Assert
        payload.Email.Should().BeEmpty();
        payload.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_WhenTokenNotValid_Throws()
    {
        // Arrange
        FacebookTokenVerifier verifier = Verifier("""{"data":{"is_valid":false,"app_id":"app-id"}}""", "{}");

        // Act
        Func<Task> act = async () => await verifier.VerifyAsync("token", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SocialTokenVerificationException>();
    }

    [Fact]
    public async Task VerifyAsync_WhenAppIdMismatches_Throws()
    {
        // Arrange — a token minted for another app is rejected
        FacebookTokenVerifier verifier = Verifier("""{"data":{"is_valid":true,"app_id":"other-app"}}""", "{}");

        // Act
        Func<Task> act = async () => await verifier.VerifyAsync("token", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SocialTokenVerificationException>();
    }

    [Fact]
    public async Task VerifyAsync_WhenProfileHasNoId_Throws()
    {
        // Arrange
        FacebookTokenVerifier verifier = Verifier(ValidDebugToken(), """{"name":"No Id"}""");

        // Act
        Func<Task> act = async () => await verifier.VerifyAsync("token", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SocialTokenVerificationException>();
    }

    /// <summary>
    /// Serves the <c>debug_token</c> and profile responses by inspecting the request path, so the
    /// verifier's two Graph calls each get their canned JSON without any network access.
    /// </summary>
    private sealed class StubHandler(string debugTokenJson, string profileJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            string path = request.RequestUri!.AbsolutePath;
            string json = path.Contains(SocialAuthConstants.FacebookDebugTokenEndpoint) ? debugTokenJson : profileJson;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            return Task.FromResult(response);
        }
    }
}
