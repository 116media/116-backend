using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.V1;

/// <summary>
/// Integration tests for the AdminForgotPassword endpoint.
/// </summary>
[Collection("Database")]
public class AdminForgotPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string ForgotPasswordUrl = $"{AuthUrl}/{AuthRouteConstants.ForgotPassword}";

    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new AdminForgotPasswordRequestBuilder().WithEmail(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ForgotPasswordUrl, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Email", Localized<ValidationErrorMessage>(m => m.EmailRequired()))
        );
    }

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

        await response.ShouldBeProblem<AccessDeniedException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InsufficientPermissions())
        );
    }

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
