using System.Security.Cryptography;
using System.Text;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;

/// <summary>
/// Integration tests for the AdminSignOut endpoint.
/// </summary>
[Collection("Database")]
public class AdminSignOutEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string SignOutUrl = $"{AuthUrl}/{AuthRouteConstants.SignOut}";
    private const string SignOutAllUrl = $"{AuthUrl}/{AuthRouteConstants.SignOutAll}";

    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task SignOut_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminSignOutRequestBuilder().WithRefreshToken("some-token").Build();

        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOutAll_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(SignOutAllUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOut_AsSuperAdmin_WithValidRefreshToken_ReturnsOk()
    {
        var rawRefreshToken = "test-refresh-token-for-signout";
        var refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        var session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            var entity = SessionFactory.CreateWithRefreshTokenHash(TestUser.SuperAdminId, refreshTokenHash);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var request = new AdminSignOutRequestBuilder().WithRefreshToken(rawRefreshToken).Build();
        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminSignOutResponse body = await response.ReadAsAsync<AdminSignOutResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var revoked = await verifyContext.Sessions.FirstAsync(s => s.Id == session.Id);
        revoked.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task SignOut_WithInactiveAccount_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var inactiveUserId = Guid.NewGuid();
        var inactiveUser = UserFactory.CreateWithId(inactiveUserId, "inactive-signout@test.com");
        inactiveUser.MarkAsVerified();
        inactiveUser.Deactivate();

        seedContext.Users.Add(inactiveUser);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(inactiveUserId, "Admin");

        var request = new AdminSignOutRequestBuilder().WithRefreshToken("some-refresh-token").Build();
        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Locked);
    }

    [Fact]
    public async Task SignOut_WithNonMatchingRefreshToken_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var request = new AdminSignOutRequestBuilder()
            .WithRefreshToken("this-token-does-not-match-any-session")
            .Build();
        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminSignOutResponse body = await response.ReadAsAsync<AdminSignOutResponse>();
        body.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SignOut_WithEmptyRefreshToken_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var request = new AdminSignOutRequestBuilder().WithRefreshToken(string.Empty).Build();
        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("RefreshToken", Localized<ValidationErrorMessage>(m => m.RefreshTokenRequired()))
        );
    }
}
