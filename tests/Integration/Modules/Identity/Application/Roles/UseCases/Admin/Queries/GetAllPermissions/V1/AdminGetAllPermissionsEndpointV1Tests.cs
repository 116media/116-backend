using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions.V1;

/// <summary>
/// Integration tests for the AdminGetAllPermissions endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllPermissionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Generates a unique resource name that fits the 15-char max length.
    /// </summary>
    private static string UniqueResource(string prefix = "pt") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    /// <summary>
    /// Generates a unique action name that fits the 15-char max length.
    /// </summary>
    private static string UniqueAction(string prefix = "act") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task GetAllPermissions_ShouldReturn200_WhenAdmin()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("ga"),
            Action = UniqueAction("ga"),
            Description = "Seeded for listing",
        };

        await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that filtering permissions by isDeleted=true returns only
    /// soft-deleted permissions.
    /// Covers PermissionIsDeletedSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllPermissions_FilterByIsDeletedTrue_ReturnsDeletedPermissions()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var deletedPermission = PermissionFactory.CreateDeleted();
        var activePermission = PermissionFactory.Create(UniqueResource("adl"), UniqueAction("adl"));
        context.Permissions.AddRange(deletedPermission, activePermission);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&isDeleted=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("permissions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isDeleted").GetBoolean().Should().BeTrue();
        }
    }

    /// <summary>
    /// Verifies that filtering permissions by isActive=false returns only
    /// inactive permissions.
    /// Covers PermissionIsNotActiveSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllPermissions_FilterByIsActiveFalse_ReturnsInactivePermissions()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var inactivePermission = PermissionFactory.CreateInactive();
        var activePermission = PermissionFactory.Create(UniqueResource("aic"), UniqueAction("aic"));
        context.Permissions.AddRange(inactivePermission, activePermission);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&isActive=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("permissions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeFalse();
        }
    }

    /// <summary>
    /// Verifies that filtering permissions by isActive=true returns only active permissions
    /// and excludes inactive ones.
    /// Covers ActivePermissionSpecification (isActive and not deleted).
    /// </summary>
    [Fact]
    public async Task GetAllPermissions_FilterByIsActiveTrue_ReturnsOnlyActivePermissions()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var activePermission = PermissionFactory.Create(UniqueResource("acp"), UniqueAction("acp"));
        var inactivePermission = PermissionFactory.CreateInactive();
        context.Permissions.AddRange(activePermission, inactivePermission);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("permissions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeTrue();
        }
    }

    /// <summary>
    /// Verifies that filtering permissions by isDeleted=false excludes soft-deleted permissions
    /// from the results.
    /// Covers PermissionNotDeletedSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllPermissions_DefaultQuery_ExcludesDeletedPermissions()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var deletedPermission = PermissionFactory.CreateDeleted();
        var activePermission = PermissionFactory.Create(UniqueResource("ndp"), UniqueAction("ndp"));
        context.Permissions.AddRange(deletedPermission, activePermission);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&isDeleted=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("permissions").GetProperty("items");

        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isDeleted").GetBoolean().Should().BeFalse();
        }
    }

    /// <summary>
    /// Verifies that filtering permissions by search term returns only
    /// permissions whose resource or action matches the search pattern.
    /// Covers PermissionSearchSpecification.
    /// </summary>
    [Fact]
    public async Task GetAllPermissions_FilterBySearch_ReturnsMatchingPermissions()
    {
        var uniqueResource = UniqueResource("xz");
        await using var context = CreateDbContext<IdentityDbContext>();
        var targetPermission = PermissionFactory.Create(uniqueResource, "read", "Searchable permission.");
        var otherPermission = PermissionFactory.Create(UniqueResource("yy"), UniqueAction("yy"));
        context.Permissions.AddRange(targetPermission, otherPermission);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&search={uniqueResource}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("permissions").GetProperty("items");

        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }
}
