using System.Net.Http.Json;
using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById.V1;

/// <summary>
/// Integration tests for the AdminGetPackageById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetPackageByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPackageById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPackageById_AsVisitor_ReturnsForbidden()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{package.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPackageById_AsSuperAdmin_WithExistingPackage_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{package.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var packageProp = doc.RootElement.GetProperty("package");

        packageProp.GetProperty("id").GetString().Should().Be(package.Id.ToString());
    }

    [Fact]
    public async Task GetPackageById_AsSuperAdmin_NonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPackageById_AsAdmin_WithExistingPackage_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{package.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
