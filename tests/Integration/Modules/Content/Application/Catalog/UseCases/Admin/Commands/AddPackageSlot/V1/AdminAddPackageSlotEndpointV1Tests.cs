using System.Net.Http.Json;
using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.V1;

/// <summary>
/// Integration tests for the AdminAddPackageSlot endpoint.
/// </summary>
[Collection("Database")]
public class AdminAddPackageSlotEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AddPackageSlot_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            CategoryId = (Guid?)null,
            IsRequired = true,
            Quantity = 1,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}/slots", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddPackageSlot_AsAdmin_ReturnsForbidden()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new
        {
            CategoryId = (Guid?)null,
            IsRequired = true,
            Quantity = 1,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/slots", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddPackageSlot_AsSuperAdmin_WithInvalidQuantity_ReturnsValidationError()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = (Guid?)null,
            IsRequired = true,
            Quantity = 0,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/slots", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddPackageSlot_AsSuperAdmin_NonExistentPackage_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = (Guid?)null,
            IsRequired = true,
            Quantity = 1,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}/slots", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddPackageSlot_AsSuperAdmin_WithValidData_ReturnsCreated()
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

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = (Guid?)category.Id,
            IsRequired = true,
            Quantity = 2,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/slots", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddPackageSlot_AsSuperAdmin_WithNullCategory_ReturnsCreated()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = (Guid?)null,
            IsRequired = false,
            Quantity = 1,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/slots", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
