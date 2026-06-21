using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;

/// <summary>
/// Integration tests for the PublicLogin endpoint.
/// </summary>
[Collection("Database")]
public class PublicLoginEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Public.Auth;

    [Fact]
    public async Task Login_WithEmptyCredentials_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Credentials = "", Password = "" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithNonExistentCredentials_ReturnsError()
    {
        Client.ClearAuthentication();
        var request = new { Credentials = "nobody@nowhere.com", Password = TestAuth.ValidPassword };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
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
        var signupRequest = new
        {
            Email = email,
            UserName = userName,
            Password = TestAuth.ValidPassword,
        };

        await Client.PostAsJsonAsync($"{AuthUrl}/signup", signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Deactivate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new { Credentials = email, Password = TestAuth.ValidPassword };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Locked);
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
        var signupRequest = new
        {
            Email = email,
            UserName = userName,
            Password = TestAuth.ValidPassword,
        };

        await Client.PostAsJsonAsync($"{AuthUrl}/signup", signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await updateContext.SaveChangesAsync();

        Client.DefaultRequestHeaders.Remove("X-Device-Id");

        var loginRequest = new { Credentials = email, Password = TestAuth.ValidPassword };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        var signupRequest = new
        {
            Email = email,
            UserName = userName,
            Password = TestAuth.ValidPassword,
        };

        await Client.PostAsJsonAsync($"{AuthUrl}/signup", signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.Activate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new { Credentials = email, Password = TestAuth.ValidPassword };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
