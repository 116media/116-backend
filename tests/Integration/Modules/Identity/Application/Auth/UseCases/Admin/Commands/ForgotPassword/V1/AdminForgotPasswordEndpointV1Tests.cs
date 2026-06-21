using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.V1;

/// <summary>
/// Integration tests for the AdminForgotPassword endpoint.
/// </summary>
[Collection("Database")]
public class AdminForgotPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/forgot-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that submitting a forgot-password request for an existing non-admin user
    /// returns 403 Forbidden, exercising the AccessDeniedExceptionHandler.
    /// The handler throws AccessDeniedException when the user lacks an admin role.
    /// </summary>
    [Fact]
    public async Task ForgotPassword_ForNonAdminUser_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorUser = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(visitorUser);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        var request = new { Email = visitorUser.Email };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/forgot-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
