namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.V1;

/// <summary>
/// Integration tests for the PublicRefreshToken endpoint.
/// </summary>
[Collection("Database")]
public class PublicRefreshTokenEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string PublicSessions = $"{ApiRoutes.Public.Base}/sessions";

    [Fact]
    public async Task RefreshToken_WithNoToken_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{PublicSessions}/refresh-token", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidTokenInBody_ReturnsForbidden()
    {
        Client.ClearAuthentication();
        var request = new { RefreshToken = "invalid-refresh-token" };

        var response = await Client.PostAsJsonAsync($"{PublicSessions}/refresh-token", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
