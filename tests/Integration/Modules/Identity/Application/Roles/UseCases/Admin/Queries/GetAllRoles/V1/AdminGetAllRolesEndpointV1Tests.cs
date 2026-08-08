using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles.V1;
using _116.Identity.Domain.Entities;
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
        List<RoleEntity> roles = RoleFactory.CreateMany(3);
        await SeedAsync<IdentityDbContext>(ctx => ctx.Roles.AddRange(roles));

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.PageIndex.Should().Be(0);
        body.Roles.PageSize.Should().Be(10);

        List<Guid> returnedIds = body.Roles.Items.Select(r => r.Id).ToList();
        returnedIds.Should().Contain(roles.Select(seeded => seeded.Id));
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
        string searchName = ShortName("srch");
        RoleEntity targetRole = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(searchName, "A role to find by search.");
            ctx.Roles.Add(entity);
            ctx.Roles.Add(RoleFactory.Create(ShortName("oth"), "This role should not match."));
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=10&search={searchName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.Items.Should().Contain(r => r.Id == targetRole.Id);
    }

    [Fact]
    public async Task GetAllRoles_WithIsActiveFilter_ReturnsOnlyMatchingRoles()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.Create(ShortName("act"), "This role is active."));
            ctx.Roles.Add(RoleFactory.CreateInactive(ShortName("ina")));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.Items.Should().OnlyContain(r => r.IsActive);
    }

    [Fact]
    public async Task GetAllRoles_FilterByIsDeletedTrue_ReturnsDeletedRoles()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateDeleted(ShortName("del")));
            ctx.Roles.Add(RoleFactory.Create(ShortName("alv"), "This role is not deleted."));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isDeleted=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.Items.Should().NotBeEmpty();
        body.Roles.Items.Should().OnlyContain(r => r.IsDeleted);
    }

    [Fact]
    public async Task GetAllRoles_FilterByIsActiveFalse_ReturnsInactiveRoles()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateInactive(ShortName("iac")));
            ctx.Roles.Add(RoleFactory.Create(ShortName("acv"), "This role is active."));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isActive=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.Items.Should().NotBeEmpty();
        body.Roles.Items.Should().OnlyContain(r => !r.IsActive);
    }

    [Fact]
    public async Task GetAllRoles_FilterByIsActiveTrue_ReturnsOnlyActiveRoles()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.Create(ShortName("actr"), "An active role."));
            ctx.Roles.Add(RoleFactory.CreateInactive(ShortName("iatr")));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.Items.Should().NotBeEmpty();
        body.Roles.Items.Should().OnlyContain(r => r.IsActive);
    }

    [Fact]
    public async Task GetAllRoles_DefaultQuery_ExcludesDeletedRoles()
    {
        string deletedRoleName = ShortName("dlt");
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateDeleted(deletedRoleName));
            ctx.Roles.Add(RoleFactory.Create(ShortName("ndl"), "This role is not deleted."));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&isDeleted=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.Items.Should().OnlyContain(r => !r.IsDeleted);
    }

    [Fact]
    public async Task GetAllRoles_FilterBySearch_ReturnsMatchingRoles()
    {
        string uniqueName = ShortName("uniq");
        RoleEntity targetRole = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(uniqueName, "A role with a unique name for search.");
            ctx.Roles.Add(entity);
            ctx.Roles.Add(RoleFactory.Create(ShortName("misc"), "This role should not match."));
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Roles}?pageIndex=0&pageSize=50&search={uniqueName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
        body.Roles.Items.Should().Contain(r => r.Id == targetRole.Id);
    }
}
