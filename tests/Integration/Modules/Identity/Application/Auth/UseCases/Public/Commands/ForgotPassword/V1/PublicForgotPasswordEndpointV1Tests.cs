using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;

/// <summary>
/// Integration tests for the PublicForgotPassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicForgotPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "not-an-email" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/forgot-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
