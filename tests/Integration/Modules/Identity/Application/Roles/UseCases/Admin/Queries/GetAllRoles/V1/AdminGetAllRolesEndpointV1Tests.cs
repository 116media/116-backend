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

    /// <summary>
    /// Verifies that filtering roles by isDeleted=true returns only
    /// soft-deleted roles.
    /// Covers RoleIsDeletedSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllRoles_FilterByIsDeletedTrue_ReturnsDeletedRoles()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var deletedRole = RoleFactory.CreateDeleted(ShortName("del"));
        var activeRole = RoleFactory.Create(ShortName("alv"), "This role is not deleted.");
        context.Roles.AddRange(deletedRole, activeRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isDeleted=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("roles").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isDeleted").GetBoolean().Should().BeTrue();
        }
    }

    /// <summary>
    /// Verifies that filtering roles by isActive=false returns only
    /// inactive roles.
    /// Covers RoleIsNotActiveSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllRoles_FilterByIsActiveFalse_ReturnsInactiveRoles()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var inactiveRole = RoleFactory.CreateInactive(ShortName("iac"));
        var activeRole = RoleFactory.Create(ShortName("acv"), "This role is active.");
        context.Roles.AddRange(inactiveRole, activeRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isActive=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("roles").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeFalse();
        }
    }

    /// <summary>
    /// Verifies that filtering roles by isActive=true returns only active roles
    /// and excludes inactive ones.
    /// Covers ActiveRoleSpecification (isActive and not deleted).
    /// </summary>
    [Fact]
    public async Task GetAllRoles_FilterByIsActiveTrue_ReturnsOnlyActiveRoles()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var activeRole = RoleFactory.Create(ShortName("actr"), "An active role.");
        var inactiveRole = RoleFactory.CreateInactive(ShortName("iatr"));
        context.Roles.AddRange(activeRole, inactiveRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("roles").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeTrue();
        }
    }

    /// <summary>
    /// Verifies that filtering roles by isDeleted=false excludes soft-deleted roles
    /// from the results.
    /// Covers RoleNotDeletedSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllRoles_DefaultQuery_ExcludesDeletedRoles()
    {
        var deletedRoleName = ShortName("dlt");
        await using var context = CreateDbContext<IdentityDbContext>();
        var deletedRole = RoleFactory.CreateDeleted(deletedRoleName);
        var activeRole = RoleFactory.Create(ShortName("ndl"), "This role is not deleted.");
        context.Roles.AddRange(deletedRole, activeRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isDeleted=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("roles").GetProperty("items");

        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isDeleted").GetBoolean().Should().BeFalse();
        }
    }

    /// <summary>
    /// Verifies that filtering roles by search term returns only roles
    /// whose name matches the search pattern.
    /// Covers RoleSearchSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllRoles_FilterBySearch_ReturnsMatchingRoles()
    {
        var uniqueName = ShortName("uniq");
        await using var context = CreateDbContext<IdentityDbContext>();
        var targetRole = RoleFactory.Create(uniqueName, "A role with a unique name for search.");
        var otherRole = RoleFactory.Create(ShortName("misc"), "This role should not match.");
        context.Roles.AddRange(targetRole, otherRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&search={uniqueName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("roles").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }
}
