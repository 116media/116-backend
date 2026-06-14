using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;

/// <summary>
/// Integration tests for the AdminLogin endpoint.
/// </summary>
[Collection("Database")]
public class AdminLoginEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "", Password = "ValidPass1" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = TestUser.SuperAdminEmail, Password = "" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsNotFoundOrBadRequest()
    {
        Client.ClearAuthentication();
        var request = new { Email = "nobody@nowhere.com", Password = "ValidPass1" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
