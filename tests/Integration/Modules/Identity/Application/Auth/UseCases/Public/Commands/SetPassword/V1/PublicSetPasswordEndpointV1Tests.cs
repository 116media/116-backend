using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;

/// <summary>
/// Integration tests for the PublicSetPassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicSetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SetPassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new PublicSetPasswordRequestBuilder().WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that an OAuth user without a password can successfully set a local password,
    /// and that the password hash is persisted and verifiable.
    /// </summary>
    [Fact]
    public async Task SetPassword_ForOAuthUser_ReturnsOk()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();

        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var oauthUser = UserFactory.CreateExternal(EnumAuthProvider.Google);

        seedContext.Users.Add(oauthUser);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(oauthUser.Id, "Visitor");

        var request = new PublicSetPasswordRequestBuilder().WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicSetPasswordResponse body = await response.ReadAsAsync<PublicSetPasswordResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var updated = await verifyContext.Users.FirstAsync(u => u.Id == oauthUser.Id);
        updated.PasswordHash.Should().NotBeNullOrWhiteSpace();
        passwordService.Verify(TestAuth.ValidPassword, updated.PasswordHash).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a local auth user trying to set a password returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task SetPassword_ForLocalUser_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var request = new PublicSetPasswordRequestBuilder().WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that submitting with an empty password returns a 400 Bad Request from the validator.
    /// </summary>
    [Fact]
    public async Task SetPassword_WithEmptyPassword_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var request = new PublicSetPasswordRequestBuilder().WithPassword(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
