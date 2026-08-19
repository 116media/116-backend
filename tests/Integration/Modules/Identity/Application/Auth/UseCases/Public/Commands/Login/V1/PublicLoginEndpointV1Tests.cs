using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using Microsoft.AspNetCore.Mvc;

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
        var request = new PublicLoginRequestBuilder().WithCredentials(string.Empty).WithPassword(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithNonExistentCredentials_ReturnsError()
    {
        Client.ClearAuthentication();
        var request = new PublicLoginRequestBuilder()
            .WithCredentials("nobody@nowhere.com")
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that a verified, active account can log in successfully and receives a
    /// fully-populated mobile token response carrying the authenticated user's details.
    /// </summary>
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokensAndUser()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"login-ok-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicLoginMobileResponse body = await response.ReadAsAsync<PublicLoginMobileResponse>();
        body.TokenType.Should().Be("Bearer");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        body.RefreshTokenExpiresAt.Should().BeAfter(body.AccessTokenExpiresAt);
        body.User.Email.Should().Be(email);
        body.User.IsActive.Should().BeTrue();
        body.User.IsVerified.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that logging in with an inactive (but verified) account returns 423 Locked.
    /// Covers the AccountInactiveExceptionHandler and UserErrors.AccountInactive() path.
    /// </summary>
    [Fact]
    public async Task Login_WithInactiveAccount_ReturnsLocked()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"inactive-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Deactivate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        await response.ShouldBeProblem(HttpStatusCode.Locked);
    }

    /// <summary>
    /// Verifies that logging in without the X-Device-Id header returns a 400 Bad Request,
    /// covering the SessionErrors.DeviceIdRequired() error path in SessionFactory.
    /// </summary>
    [Fact]
    public async Task Login_WithoutDeviceIdHeader_ReturnsBadRequest()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"nodevice-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await updateContext.SaveChangesAsync();

        Client.DefaultRequestHeaders.Remove("X-Device-Id");

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that logging in with an unverified (but active) account returns 403 Forbidden.
    /// Covers the AccountNotVerifiedExceptionHandler and UserErrors.AccountNotVerified() path.
    /// </summary>
    [Fact]
    public async Task Login_WithUnverifiedAccount_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"unverified-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.Activate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        await response.ShouldBeProblem(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that a known account rejected on its password reaches
    /// <c>PublicLoginAuthFactory</c>'s <c>InvalidCredentials</c> branch: the credentials lookup
    /// succeeds, <c>IPasswordService.Verify</c> fails, and the response is a 401 carrying the
    /// neutral credential message — distinct from the 404 a nonexistent account produces.
    /// </summary>
    [Fact]
    public async Task Login_WithKnownEmailAndWrongPassword_ReturnsInvalidCredentialsUnauthorized()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        UserErrors errors = TestErrorsFactory.CreateUserErrors();

        var email = $"wrong-password-{Guid.NewGuid():N}@test.com";
        var user = UserFactory.Create(email);
        user.MarkAsVerified();
        user.Activate();
        user.InitializePasswordHash(passwordService.Hash(TestAuth.ValidPassword), errors);

        await SeedAsync<IdentityDbContext>(context => context.Users.Add(user));

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var request = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword($"{TestAuth.ValidPassword}-not-it")
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);

        await response.ShouldBeProblem(HttpStatusCode.Unauthorized, "Invalid email or password.");

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
        problem.Detail.Should().NotContain("user account");
    }

    /// <summary>
    /// Reproduces the reported bug: a non-existent login (Accept-Language: en) must
    /// return a friendly English message that never leaks the raw entity class name,
    /// the "credentials" key, or the searched email.
    /// </summary>
    [Fact]
    public async Task Login_WithNonExistentCredentials_ReturnsFriendlyDetailWithoutLeakingEmail()
    {
        Client.ClearAuthentication();
        const string email = "ghost-en@nowhere.com";
        var request = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, Routes.Public.Auth.Login())
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Add("Accept-Language", "en");

        var response = await Client.SendAsync(httpRequest);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
        problem.Detail.Should().Contain("user account");
        problem.Detail.Should().NotContain(email);
        problem.Detail.Should().NotContain("credentials");
        problem.Detail.Should().NotContain("User with");
    }

    /// <summary>
    /// Verifies the localization fix end-to-end: with Accept-Language: fr the same
    /// not-found returns the friendly French message — proving request localization now
    /// wraps the exception handler — still without leaking the searched email.
    /// </summary>
    [Fact]
    public async Task Login_WithNonExistentCredentials_InFrench_ReturnsLocalizedFriendlyDetail()
    {
        Client.ClearAuthentication();
        const string email = "fantome-fr@nowhere.com";
        var request = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, Routes.Public.Auth.Login())
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Add("Accept-Language", "fr");

        var response = await Client.SendAsync(httpRequest);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
        problem.Detail.Should().Contain("Impossible de trouver");
        problem.Detail.Should().Contain("compte utilisateur");
        problem.Detail.Should().NotContain(email);
    }
}
