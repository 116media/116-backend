using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;

/// <summary>
/// Integration tests for the PublicLogin endpoint.
/// </summary>
[Collection("Database")]
public class PublicLoginEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Login_WithEmptyCredentials_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Credentials = "", Password = "" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithNonExistentCredentials_ReturnsError()
    {
        Client.ClearAuthentication();
        var request = new { Credentials = "nobody@nowhere.com", Password = TestAuth.ValidPassword };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
