using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

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
}
