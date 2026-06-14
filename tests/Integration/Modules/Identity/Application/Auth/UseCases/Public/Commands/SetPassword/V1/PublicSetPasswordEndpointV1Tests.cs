using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;

/// <summary>
/// Integration tests for the PublicSetPassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicSetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SetPassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Password = Auth.ValidPassword };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/set-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
