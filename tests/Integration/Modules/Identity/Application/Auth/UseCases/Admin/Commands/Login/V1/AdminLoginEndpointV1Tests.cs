using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;

/// <summary>
/// Integration tests for the AdminLogin endpoint.
/// </summary>
[Collection("Database")]
public class AdminLoginEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string LoginUrl = $"{AuthUrl}/{AuthRouteConstants.Login}";

    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new AdminLoginRequestBuilder().WithEmail(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(LoginUrl, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Email", Localized<ValidationErrorMessage>(m => m.EmailRequired()))
        );
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new AdminLoginRequestBuilder()
            .WithEmail(TestUser.SuperAdminEmail)
            .WithPassword(string.Empty)
            .Build();

        var response = await Client.PostAsJsonAsync(LoginUrl, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Password", Localized<ValidationErrorMessage>(m => m.PasswordRequired()))
        );
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsInvalidCredentialsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminLoginRequestBuilder().WithEmail("nobody@nowhere.com").Build();

        var response = await Client.PostAsJsonAsync(LoginUrl, request);

        await response.ShouldBeProblem<AuthenticationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials())
        );
    }

    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsAdminUser()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        string hashedPassword = passwordService.Hash(TestAuth.ValidPassword);
        var errors = TestErrorsFactory.CreateUserErrors();

        var email = $"admin-login-ok-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.MarkAsVerified();
        user.Activate();
        user.InitializePasswordHash(hashedPassword, errors);
        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.Add(adminRole);
        seedContext.Users.Add(user);
        seedContext.UserRoles.Add(userRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var request = new AdminLoginRequestBuilder().WithEmail(email).WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(LoginUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminLoginResponse body = await response.ReadAsAsync<AdminLoginResponse>();
        body.User.Id.Should().Be(user.Id);
        body.User.Email.Should().Be(request.Email);
        body.User.IsActive.Should().BeTrue();
        body.User.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithKnownEmailAndWrongPassword_ReturnsInvalidCredentialsUnauthorized()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        var errors = TestErrorsFactory.CreateUserErrors();

        var email = $"admin-wrong-password-{Guid.NewGuid():N}@test.com";
        var adminRole = RoleFactory.CreateAdmin();
        var user = UserFactory.Create(email);
        user.MarkAsVerified();
        user.Activate();
        user.InitializePasswordHash(passwordService.Hash(TestAuth.ValidPassword), errors);
        var userRole = UserRoleFactory.Create(user.Id, adminRole.Id);

        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Roles.Add(adminRole);
            context.Users.Add(user);
            context.UserRoles.Add(userRole);
        });

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var request = new AdminLoginRequestBuilder()
            .WithEmail(email)
            .WithPassword($"{TestAuth.ValidPassword}-not-it")
            .Build();

        var response = await Client.PostAsJsonAsync(LoginUrl, request);

        await response.ShouldBeProblem<AuthenticationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials(), LocalizedMessage.EnglishCulture)
        );
    }
}
