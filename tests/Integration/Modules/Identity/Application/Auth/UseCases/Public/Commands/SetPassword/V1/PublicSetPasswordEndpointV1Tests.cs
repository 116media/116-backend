using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;
using _116.Identity.Domain.Entities;
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
    /// Verifies that an OAuth account whose provider shared no email address is refused a local
    /// password. A local credential without an address would be unrecoverable — no reset could
    /// ever reach the owner — so the request is rejected before any hash is written.
    /// </summary>
    [Fact]
    public async Task SetPassword_ForOAuthUserWithoutAnEmailAddress_ReturnsBadRequest()
    {
        UserEntity oauthUser = await SeedAsync<IdentityDbContext, UserEntity>(context =>
        {
            UserEntity created = UserFactory.CreateExternalWithoutEmail(EnumAuthProvider.Facebook);
            context.Users.Add(created);
            return created;
        });

        Client.AuthenticateAs(oauthUser.Id, "Visitor");
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var request = new PublicSetPasswordRequestBuilder().WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest, "An email address is required to set a password.");

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity untouched = await verifyContext.Users.FirstAsync(u => u.Id == oauthUser.Id);
        untouched.PasswordHash.Should().BeNull();
        untouched.AuthProvider.Should().Be(EnumAuthProvider.Facebook);
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
