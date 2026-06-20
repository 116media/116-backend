using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles.V1;

/// <summary>
/// Integration tests for the AdminGetAllRoles endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllRolesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "rq") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task GetAllRoles_AsSuperAdmin_ReturnsOkWithItems()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var roles = RoleFactory.CreateMany(3);
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("roles", out var roles_prop).Should().BeTrue();
        roles_prop.TryGetProperty("items", out var items).Should().BeTrue();
        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllRoles_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllRoles_WithoutAuthentication_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllRoles_WithSearchParam_ReturnsFilteredResults()
    {
        var searchName = ShortName("srch");
        await using var context = CreateDbContext<IdentityDbContext>();
        var targetRole = RoleFactory.Create(searchName, "A role to find by search.");
        var otherRole = RoleFactory.Create(ShortName("oth"), "This role should not match.");
        context.Roles.AddRange(targetRole, otherRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=10&search={searchName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var roles_prop = doc.RootElement.GetProperty("roles");
        var items = roles_prop.GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAllRoles_WithIsActiveFilter_ReturnsOnlyMatchingRoles()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var activeRole = RoleFactory.Create(ShortName("act"), "This role is active.");
        var inactiveRole = RoleFactory.CreateInactive(ShortName("ina"));
        context.Roles.AddRange(activeRole, inactiveRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var roles_prop = doc.RootElement.GetProperty("roles");
        var items = roles_prop.GetProperty("items");

        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeTrue();
        }
    }
}
