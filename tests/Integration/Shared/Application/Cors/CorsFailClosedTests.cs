namespace _116.Integration.Tests.Shared.Application.Cors;

/// <summary>
/// Verifies that the default host — booted outside Development with no origins configured — fails CORS
/// closed: a cross-origin preflight receives no <c>Access-Control-Allow-Origin</c> header, rather than
/// the any-origin policy the host previously fell back to.
/// </summary>
/// <param name="db">The shared Testcontainer database and default application host.</param>
[Collection("Database")]
public class CorsFailClosedTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Preflight_WhenNoOriginsConfigured_DoesNotEchoAllowOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, Routes.Public.Auth.Login());
        request.Headers.Add("Origin", "https://anything.example");
        request.Headers.Add("Access-Control-Request-Method", HttpMethod.Post.Method);

        using HttpResponseMessage response = await Client.SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }
}
