using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;

/// <summary>
/// Integration tests for the AdminSignOut endpoint.
/// </summary>
[Collection("Database")]
public class AdminSignOutEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task SignOut_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { RefreshToken = "some-token" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/sign-out", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOutAll_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{AuthUrl}/sign-out-all", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
