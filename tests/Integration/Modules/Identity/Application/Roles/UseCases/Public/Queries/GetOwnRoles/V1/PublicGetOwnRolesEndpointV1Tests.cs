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

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnRoles_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOwnRoles_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
