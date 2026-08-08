using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.User.Constants;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.V1;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.V1;

/// <summary>
/// Integration tests for the AdminUpdateOwnProfile endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateOwnProfileEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/{IdentityConstants.Me}/{UserRouteConstants.Profile}";

    [Fact]
    public async Task AdminUpdateOwnProfile_AsSuperAdmin_WithValidSession_UpdatesProfile()
    {
        var sessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.SuperAdminId));
        });

        Client.AuthenticateAs(TestUser.SuperAdminId, "SuperAdmin", sessionId);

        // Country fields only persist alongside a phone-number update, so the builder's
        // valid default carries the full phone + country set to exercise the country update path.
        var request = new AdminUpdateOwnProfileRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(AdminMeProfile, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateOwnProfileResponse body = await response.ReadAsAsync<AdminUpdateOwnProfileResponse>();
        body.User.Id.Should().Be(TestUser.SuperAdminId);
        body.User.UserName.Should().Be(request.UserName);
        body.User.CountryName.Should().Be(request.CountryName);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity? user = await verifyContext.Users.FindAsync(TestUser.SuperAdminId);
        user!.UserName.Should().Be(request.UserName);
        user.CountryName.Should().Be(request.CountryName);
        user.CountryDialCode.Should().Be(request.CountryDialCode);
    }

    [Fact]
    public async Task AdminUpdateOwnProfile_AsSuperAdmin_WithoutValidSession_ReturnsForbidden()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdateOwnProfileRequest(
            UserName: "newname123",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        var response = await Client.PatchAsJsonAsync(AdminMeProfile, request);

        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );
    }

    [Fact]
    public async Task AdminUpdateOwnProfile_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();
        var request = new AdminUpdateOwnProfileRequest(
            UserName: "noauth123",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        var response = await Client.PatchAsJsonAsync(AdminMeProfile, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
