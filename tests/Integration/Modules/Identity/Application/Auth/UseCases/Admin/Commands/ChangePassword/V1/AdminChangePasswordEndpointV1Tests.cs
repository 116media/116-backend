using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword.V1;

/// <summary>
/// Integration tests for the AdminChangePassword endpoint.
/// </summary>
[Collection("Database")]
public class AdminChangePasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task ChangePassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { OldPassword = "OldPass1", NewPassword = "NewPass1" };

        var response = await Client.PatchAsJsonAsync($"{AuthUrl}/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithWeakPassword_ReturnsValidationError()
    {
        Client.AuthenticateAsAdmin();
        var request = new { OldPassword = "OldPass1", NewPassword = "weak" };

        var response = await Client.PatchAsJsonAsync($"{AuthUrl}/change-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that sending an empty old and new password returns a 400 Bad Request
    /// due to validation failure from the AdminChangePasswordValidator.
    /// </summary>
    [Fact]
    public async Task ChangePassword_WithEmptyPayload_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var request = new { OldPassword = "", NewPassword = "" };
        var response = await Client.PatchAsJsonAsync($"{AuthUrl}/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
