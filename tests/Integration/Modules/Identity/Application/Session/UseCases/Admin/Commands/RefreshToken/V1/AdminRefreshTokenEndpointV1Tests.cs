namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken.V1;

/// <summary>
/// Integration tests for the AdminRefreshToken endpoint.
/// </summary>
[Collection("Database")]
public class AdminRefreshTokenEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RefreshToken_WithNoToken_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/refresh-token", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidTokenInBody_ReturnsForbidden()
    {
        Client.ClearAuthentication();
        var request = new { RefreshToken = "invalid-refresh-token" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Sessions}/refresh-token", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
