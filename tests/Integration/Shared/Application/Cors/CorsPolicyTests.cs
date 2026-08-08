namespace _116.Integration.Tests.Shared.Application.Cors;

/// <summary>
/// Verifies that the default CORS policy built in <c>Program.cs</c> from
/// <c>AppEnvironment.CorsAllowedOrigins</c> admits a configured origin with credentials and does
/// not echo an unconfigured one.
/// </summary>
/// <remarks>
/// These tests run against <see cref="CorsPostgresFixture" />, the only host booted with an
/// allowed origin configured, so the populated branch of the policy is the one exercised. The
/// requests are preflights: they are answered by the CORS middleware before authentication and
/// before the endpoint runs, which is why an unauthenticated request against a protected route
/// still proves the policy.
/// </remarks>
/// <param name="db">The dedicated Testcontainer database and CORS-configured application host.</param>
[Collection("Cors")]
public class CorsPolicyTests(CorsPostgresFixture db) : IDisposable
{
    private readonly HttpClient _client = db.Api.CreateClient();

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Preflight_FromConfiguredOrigin_EchoesAllowOriginAndAllowsCredentials()
    {
        using HttpResponseMessage response = await SendPreflightAsync(CorsApiFixture.AllowedOrigin);

        response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle(CorsApiFixture.AllowedOrigin);
        response.Headers.GetValues("Access-Control-Allow-Credentials").Should().ContainSingle("true");
    }

    [Fact]
    public async Task Preflight_FromUnconfiguredOrigin_DoesNotEchoAllowOrigin()
    {
        using HttpResponseMessage response = await SendPreflightAsync(CorsApiFixture.UnconfiguredOrigin);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    /// <summary>
    /// Sends a CORS preflight for a cross-origin POST to the public login route.
    /// </summary>
    /// <param name="origin">The origin the preflight claims to come from.</param>
    /// <returns>The preflight response.</returns>
    private async Task<HttpResponseMessage> SendPreflightAsync(string origin)
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, Routes.Public.Auth.Login());
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", HttpMethod.Post.Method);

        return await _client.SendAsync(request);
    }
}
