using System.Net.Http.Json;
using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot.V1;

/// <summary>
/// Integration tests for the AdminRemovePackageSlot endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemovePackageSlotEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RemovePackageSlot_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}/slots/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemovePackageSlot_AsAdmin_ReturnsForbidden()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        var slot = PackageSlotFactory.Create(package.Id);
        context.PackageSlots.Add(slot);
        await context.SaveChangesAsync();

        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/slots/{slot.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemovePackageSlot_AsSuperAdmin_NonExistentPackage_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}/slots/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that removing a package slot that does not exist on a valid package
    /// returns a 404 Not Found response.
    /// </summary>
    [Fact]
    public async Task RemovePackageSlot_NonExistentSlot_ReturnsNotFound()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/slots/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemovePackageSlot_AsSuperAdmin_WithExistingSlot_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        var slot = PackageSlotFactory.Create(package.Id, category.Id);
        context.PackageSlots.Add(slot);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/slots/{slot.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
