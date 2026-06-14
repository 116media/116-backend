using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignOut.V1;

/// <summary>
/// Integration tests for the PublicSignOut endpoint.
/// </summary>
[Collection("Database")]
public class PublicSignOutEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SignOut_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { RefreshToken = "" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/sign-out", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOut_AsVisitor_WithEmptyRefreshToken_ReturnsValidationError()
    {
        Client.AuthenticateAsVisitor();
        var request = new { RefreshToken = "" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/sign-out", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SignOutAll_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Auth}/sign-out-all", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
