using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.V1;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.V1;

/// <summary>
/// Integration tests for the AdminForgotPassword endpoint.
/// </summary>
[Collection("Database")]
public class AdminForgotPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string ForgotPasswordUrl = $"{AuthUrl}/{AuthRouteConstants.ForgotPassword}";

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new AdminForgotPasswordRequestBuilder().WithEmail(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ForgotPasswordUrl, request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
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
        var request = new AdminForgotPasswordRequestBuilder().WithEmail(visitorUser.Email!).Build();

        var response = await Client.PostAsJsonAsync(ForgotPasswordUrl, request);

        await response.ShouldBeProblem(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that a forgot-password request for an existing admin account returns 200 OK
    /// with the anti-enumeration success payload echoing the submitted email.
    /// </summary>
    [Fact]
    public async Task ForgotPassword_ForAdminUser_ReturnsSuccessEchoingEmail()
    {
        var email = $"admin-forgot-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.MarkAsVerified();
        user.Activate();
        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.Add(adminRole);
        seedContext.Users.Add(user);
        seedContext.UserRoles.Add(userRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        var request = new AdminForgotPasswordRequestBuilder().WithEmail(email).Build();

        var response = await Client.PostAsJsonAsync(ForgotPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminForgotPasswordResponse body = await response.ReadAsAsync<AdminForgotPasswordResponse>();
        body.IsSuccess.Should().BeTrue();
        body.Email.Should().Be(request.Email);
    }
}
