using _116.Identity.Application.User.Constants;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile.V1;
using _116.Identity.Domain.Constants;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile.V1;

/// <summary>
/// Integration tests for the AdminGetOwnProfile endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOwnProfileEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/{IdentityConstants.Me}/{UserRouteConstants.Profile}";

    [Fact]
    public async Task AdminGetOwnProfile_AsSuperAdmin_Returns200WithUser()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(AdminMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetOwnProfileResponse body = await response.ReadAsAsync<AdminGetOwnProfileResponse>();
        body.User.Id.Should().Be(TestUser.SuperAdminId);
        body.User.Email.Should().Be(TestUser.SuperAdminEmail);
    }

    [Fact]
    public async Task AdminGetOwnProfile_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(AdminMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetOwnProfile_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(AdminMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
