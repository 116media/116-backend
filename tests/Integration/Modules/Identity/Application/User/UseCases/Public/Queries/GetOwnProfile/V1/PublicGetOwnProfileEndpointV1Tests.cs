using _116.Identity.Application.User.UseCases.Public.Queries.GetOwnProfile.V1;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Public.Queries.GetOwnProfile.V1;

/// <summary>
/// Integration tests for the PublicGetOwnProfile endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnProfileEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublicGetOwnProfile_AsVisitor_Returns200()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(Routes.Public.Me.Profile());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetOwnProfileResponse body = await response.ReadAsAsync<PublicGetOwnProfileResponse>();
        body.User.Id.Should().Be(TestUser.VisitorId);
        body.User.Email.Should().Be(TestUser.VisitorEmail);
    }

    [Fact]
    public async Task PublicGetOwnProfile_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Me.Profile());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublicGetOwnProfile_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(Routes.Public.Me.Profile());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
