using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword.V1;

/// <summary>
/// Integration tests for the PublicChangePassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicChangePasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ChangePassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { OldPassword = "Old123!abc", NewPassword = "New123!abc" };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.Public.Auth}/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { OldPassword = "Old123!abc", NewPassword = "New123!abc" };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.Public.Auth}/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
