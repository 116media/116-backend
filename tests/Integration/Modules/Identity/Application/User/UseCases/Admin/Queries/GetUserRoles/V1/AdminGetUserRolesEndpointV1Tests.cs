using System.Text.Json;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles.V1;

/// <summary>
/// Integration tests for the AdminGetUserRoles endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetUserRolesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/me/profile";
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/me/avatar";
    private const string PublicMeProfile = $"{ApiRoutes.Public.Me}/profile";
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task AdminGetUserRoles_AsSuperAdmin_Returns200()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("roles", out _).Should().BeTrue();
    }

    [Fact]
    public async Task AdminGetUserRoles_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetUserRoles_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
