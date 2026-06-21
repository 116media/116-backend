using System.Net.Http.Json;
using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage.V1;

/// <summary>
/// Integration tests for the AdminDeactivatePackage endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivatePackageEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeactivatePackage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivatePackage_AsVisitor_ReturnsForbidden()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivatePackage_AsSuperAdmin_WithExistingActivePackage_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivatePackage_AsSuperAdmin_NonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivatePackage_AsSuperAdmin_AlreadyInactive_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.CreateInactive();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
