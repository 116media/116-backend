using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword.V1;

/// <summary>
/// Integration tests for the AdminChangePassword endpoint.
/// </summary>
[Collection("Database")]
public class AdminChangePasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string ChangePasswordUrl = $"{AuthUrl}/{AuthRouteConstants.ChangePassword}";
    private const string KnownPassword = TestAuth.ValidPassword;

    private static string ValidationDetail(params (string Property, string Message)[] failures) =>
        new ValidationException(failures.Select(f => new ValidationFailure(f.Property, f.Message))).Message;

    [Fact]
    public async Task ChangePassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminChangePasswordRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithWeakPassword_ReturnsValidationError()
    {
        Client.AuthenticateAsAdmin();
        var request = new AdminChangePasswordRequestBuilder().WithNewPassword("weak").Build();

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                (
                    "NewPassword",
                    Localized<ValidationErrorMessage>(m =>
                        m.PasswordTooShort(m.NewPasswordFieldName(), UserConstants.MinPasswordLength)
                    )
                )
            )
        );
    }

    [Fact]
    public async Task ChangePassword_WithEmptyPayload_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var request = new AdminChangePasswordRequestBuilder()
            .WithOldPassword(string.Empty)
            .WithNewPassword(string.Empty)
            .Build();
        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                ("OldPassword", Localized<ValidationErrorMessage>(m => m.CurrentPasswordRequired())),
                ("NewPassword", Localized<ValidationErrorMessage>(m => m.PasswordRequired()))
            )
        );
    }

    [Fact]
    public async Task ChangePassword_AsAdmin_WithCorrectPassword_ReturnsOk()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        string hashedPassword = passwordService.Hash(KnownPassword);
        var errors = TestErrorsFactory.CreateUserErrors();

        var email = $"admin-chg-ok-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.MarkAsVerified();
        user.Activate();
        user.InitializePasswordHash(hashedPassword, errors);
        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, user.Id);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.Add(adminRole);
        seedContext.Users.Add(user);
        seedContext.UserRoles.Add(userRole);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(user.Id, "Admin", sessionId);

        var request = new AdminChangePasswordRequestBuilder()
            .WithOldPassword(KnownPassword)
            .WithNewPassword(TestAuth.ChangedPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminChangePasswordResponse body = await response.ReadAsAsync<AdminChangePasswordResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var updated = await verifyContext.Users.FirstAsync(u => u.Id == user.Id);
        passwordService.Verify(request.NewPassword, updated.PasswordHash).Should().BeTrue();
    }
}
