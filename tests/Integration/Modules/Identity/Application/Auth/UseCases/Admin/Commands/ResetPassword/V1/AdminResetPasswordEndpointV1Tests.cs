using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.V1;

/// <summary>
/// Integration tests for the AdminResetPassword endpoint.
/// </summary>
[Collection("Database")]
public class AdminResetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task ResetPassword_WithEmptyFields_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "",
            Code = "",
            NewPassword = "",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/reset-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
