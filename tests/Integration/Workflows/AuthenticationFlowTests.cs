using System.Net.Http.Headers;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.User.UseCases.Public.Queries.GetOwnProfile.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Cross-module workflow tests for the authentication lifecycle:
/// signup → login → access protected endpoint → signout.
/// </summary>
[Collection("Database")]
public class AuthenticationFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SignUp_PersistsTheUserAndReturnsTokens()
    {
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), nameof(EnumCoreUserRole.Visitor)))
        );

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        string email = $"flow-{Guid.NewGuid():N}@test.com";
        string userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequest(Email: email, UserName: userName, Password: TestAuth.ValidPassword);

        HttpResponseMessage signupResponse = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);
        signupResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        PublicSignUpMobileResponse signupBody = await signupResponse.ReadAsAsync<PublicSignUpMobileResponse>();
        signupBody.AccessToken.Should().NotBeNullOrEmpty();
        signupBody.RefreshToken.Should().NotBeNullOrEmpty();
        signupBody.User.Email.Should().Be(email);
        signupBody.User.UserName.Should().Be(userName);

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.Users.AnyAsync(u => u.Id == signupBody.User.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), nameof(EnumCoreUserRole.Visitor)))
        );

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        string email = $"login-{Guid.NewGuid():N}@test.com";
        string userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequest(Email: email, UserName: userName, Password: TestAuth.ValidPassword);

        HttpResponseMessage signupResponse = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);
        signupResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        PublicSignUpMobileResponse signupBody = await signupResponse.ReadAsAsync<PublicSignUpMobileResponse>();
        Guid userId = signupBody.User.Id;

        await using (IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>())
        {
            var user = await verifyContext.Users.FirstAsync(u => u.Id == userId);
            user.MarkAsVerified();
            user.Activate();
            await verifyContext.SaveChangesAsync();
        }

        var loginRequest = new PublicLoginRequest(Credentials: email, Password: TestAuth.ValidPassword);

        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicLoginMobileResponse loginBody = await loginResponse.ReadAsAsync<PublicLoginMobileResponse>();
        loginBody.AccessToken.Should().NotBeNullOrEmpty();
        loginBody.RefreshToken.Should().NotBeNullOrEmpty();
        loginBody.AccessToken.Split('.').Should().HaveCount(3);
        loginBody.User.Id.Should().Be(userId);
        loginBody.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_ThenCallProtectedEndpointWithTheIssuedToken_ResolvesTheCaller()
    {
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), nameof(EnumCoreUserRole.Visitor)))
        );

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        string email = $"issued-{Guid.NewGuid():N}@test.com";
        string userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequest(Email: email, UserName: userName, Password: TestAuth.ValidPassword);

        HttpResponseMessage signupResponse = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);
        signupResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using (IdentityDbContext seedContext = CreateDbContext<IdentityDbContext>())
        {
            UserEntity user = await seedContext.Users.FirstAsync(u => u.Email == email);
            user.MarkAsVerified();
            user.Activate();
            await seedContext.SaveChangesAsync();
        }

        var loginRequest = new PublicLoginRequest(Credentials: email, Password: TestAuth.ValidPassword);

        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicLoginMobileResponse loginBody = await loginResponse.ReadAsAsync<PublicLoginMobileResponse>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.AccessToken);

        HttpResponseMessage protectedResponse = await Client.GetAsync(Routes.Public.Me.Profile());

        protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetOwnProfileResponse profile = await protectedResponse.ReadAsAsync<PublicGetOwnProfileResponse>();
        profile.User.Email.Should().Be(email, "the endpoint resolved the caller from the issued token's claims");
    }

    [Fact]
    public async Task SignUp_WithDuplicateEmail_ShouldReturnConflict()
    {
        Client.ClearAuthentication();

        var request = new PublicSignUpRequest(
            Email: TestUser.SuperAdminEmail,
            UserName: $"u{Guid.NewGuid():N}"[..10],
            Password: TestAuth.ValidPassword
        );

        HttpResponseMessage response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.EmailAlreadyExists(TestUser.SuperAdminEmail))
        );
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnError()
    {
        Client.ClearAuthentication();

        var request = new PublicLoginRequest(Credentials: "nonexistent@nobody.com", Password: TestAuth.ValidPassword);

        HttpResponseMessage response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("User"))
        );
    }
}
