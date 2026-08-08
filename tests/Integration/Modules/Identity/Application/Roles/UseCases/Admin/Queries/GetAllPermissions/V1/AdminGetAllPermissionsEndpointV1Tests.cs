using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions.V1;
using _116.Identity.Domain.Entities;
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
        PermissionEntity seeded = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.Create(
                UniqueResource("ga"),
                UniqueAction("ga"),
                "Seeded for listing"
            );
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPermissionsResponse>();
        body.Permissions.PageIndex.Should().Be(0);
        body.Permissions.Items.Should().Contain(p => p.Id == seeded.Id);
    }

    [Fact]
    public async Task GetAllPermissions_FilterByIsDeletedTrue_ReturnsDeletedPermissions()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Permissions.Add(PermissionFactory.CreateDeleted());
            ctx.Permissions.Add(PermissionFactory.Create(UniqueResource("adl"), UniqueAction("adl")));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&isDeleted=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPermissionsResponse>();
        body.Permissions.Items.Should().NotBeEmpty();
        body.Permissions.Items.Should().OnlyContain(p => p.IsDeleted);
    }

    [Fact]
    public async Task GetAllPermissions_FilterByIsActiveFalse_ReturnsInactivePermissions()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Permissions.Add(PermissionFactory.CreateInactive());
            ctx.Permissions.Add(PermissionFactory.Create(UniqueResource("aic"), UniqueAction("aic")));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&isActive=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPermissionsResponse>();
        body.Permissions.Items.Should().NotBeEmpty();
        body.Permissions.Items.Should().OnlyContain(p => !p.IsActive);
    }

    [Fact]
    public async Task GetAllPermissions_FilterByIsActiveTrue_ReturnsOnlyActivePermissions()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Permissions.Add(PermissionFactory.Create(UniqueResource("acp"), UniqueAction("acp")));
            ctx.Permissions.Add(PermissionFactory.CreateInactive());
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPermissionsResponse>();
        body.Permissions.Items.Should().NotBeEmpty();
        body.Permissions.Items.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetAllPermissions_DefaultQuery_ExcludesDeletedPermissions()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Permissions.Add(PermissionFactory.CreateDeleted());
            ctx.Permissions.Add(PermissionFactory.Create(UniqueResource("ndp"), UniqueAction("ndp")));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&isDeleted=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPermissionsResponse>();
        body.Permissions.Items.Should().OnlyContain(p => !p.IsDeleted);
    }

    [Fact]
    public async Task GetAllPermissions_FilterBySearch_ReturnsMatchingPermissions()
    {
        string uniqueResource = UniqueResource("xz");
        PermissionEntity targetPermission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.Create(uniqueResource, "read", "Searchable permission.");
            ctx.Permissions.Add(entity);
            ctx.Permissions.Add(PermissionFactory.Create(UniqueResource("yy"), UniqueAction("yy")));
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Permissions}?pageIndex=0&pageSize=50&search={uniqueResource}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPermissionsResponse>();
        body.Permissions.Items.Should().Contain(p => p.Id == targetPermission.Id);
    }
}
