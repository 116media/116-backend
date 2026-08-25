using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages.V1;

/// <summary>
/// Integration tests for the AdminGetAllPackages endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllPackagesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllPackages_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllPackages_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllPackages_AsSuperAdmin_ReturnsOkWithItems()
    {
        List<PackageEntity> packages = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            packages = PackageFactory.CreateMany(3);
            ctx.Packages.AddRange(packages);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPackagesResponse>();
        body.Packages.PageIndex.Should().Be(0);
        body.Packages.PageSize.Should().Be(10);
        body.Packages.Count.Should().Be(3);
        body.Packages.Items.Should().Contain(p => p.Id == packages[0].Id);
    }

    [Fact]
    public async Task GetAllPackages_AsAdmin_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPackagesResponse>();
        body.Packages.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllPackages_WithIsActiveFilter_ReturnsFilteredResults()
    {
        PackageEntity activePackage = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            activePackage = PackageFactory.Create();
            PackageEntity inactivePackage = PackageFactory.CreateInactive();
            ctx.Packages.AddRange(activePackage, inactivePackage);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPackagesResponse>();
        body.Packages.Items.Should().OnlyContain(p => p.IsActive);
        body.Packages.Items.Should().Contain(p => p.Id == activePackage.Id);
    }
}
