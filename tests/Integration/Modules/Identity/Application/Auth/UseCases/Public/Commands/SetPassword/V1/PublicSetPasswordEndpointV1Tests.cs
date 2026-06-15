using System.Text.Json;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;

/// <summary>
/// Integration tests for the PublicSetPassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicSetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string SetPasswordUrl = $"{ApiRoutes.Public.Auth}/set-password";

    [Fact]
    public async Task SetPassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Password = TestAuth.ValidPassword };

        var response = await Client.PostAsJsonAsync(SetPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that an OAuth user without a password can successfully set a local password.
    /// </summary>
    [Fact]
    public async Task SetPassword_ForOAuthUser_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var oauthUser = UserFactory.CreateExternal(EnumAuthProvider.Google);

        seedContext.Users.Add(oauthUser);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(oauthUser.Id, "Visitor");

        var request = new { Password = TestAuth.ValidPassword };

        var response = await Client.PostAsJsonAsync(SetPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that a local auth user trying to set a password returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task SetPassword_ForLocalUser_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var request = new { Password = TestAuth.ValidPassword };

        var response = await Client.PostAsJsonAsync(SetPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that submitting with an empty password returns a 400 Bad Request from the validator.
    /// </summary>
    [Fact]
    public async Task SetPassword_WithEmptyPassword_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var request = new { Password = "" };

        var response = await Client.PostAsJsonAsync(SetPasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
