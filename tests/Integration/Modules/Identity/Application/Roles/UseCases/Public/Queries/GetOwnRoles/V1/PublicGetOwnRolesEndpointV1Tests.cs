using _116.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles.V1;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles.V1;

/// <summary>
/// Integration tests for the PublicGetOwnRoles endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnRolesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetOwnRoles_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Me.Roles());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnRoles_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(Routes.Public.Me.Roles());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetOwnRolesResponse>();
        body.Roles.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOwnRoles_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(Routes.Public.Me.Roles());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
