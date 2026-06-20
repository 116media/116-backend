using System.Net.Http.Json;
using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Content.Api.Catalog;

/// <summary>
/// Integration tests for the admin package endpoints covering create, activate, deactivate,
/// add slot, remove slot, get all, and get by ID operations against a real PostgreSQL database
/// through the full API pipeline.
/// </summary>
[Collection("Database")]
public class AdminPackageEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreatePackage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Unauthorized Pkg", Description = "Should not be created" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePackage_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { Name = "Forbidden Pkg", Description = "Admin cannot create" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "", Description = "Missing name" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithEmptyDescription_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Valid Name", Description = "" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Premium Bundle", Description = "A premium content package" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var context = CreateDbContext<ContentDbContext>();
        var package = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Premium Bundle");
        package.Should().NotBeNull();
        package!.Description.Should().Be("A premium content package");
    }

    [Fact]
    public async Task ActivatePackage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActivatePackage_AsVisitor_ReturnsForbidden()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.CreateInactive();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ActivatePackage_AsSuperAdmin_WithExistingInactivePackage_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.CreateInactive();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{package.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivatePackage_AsSuperAdmin_NonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

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
        await using var context = CreateDbContext<ContentDbContext>();
        var packages = PackageFactory.CreateMany(3);
        context.Packages.AddRange(packages);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("packages", out var packagesProp).Should().BeTrue();
        packagesProp.TryGetProperty("items", out var items).Should().BeTrue();
        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllPackages_AsAdmin_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllPackages_WithIsActiveFilter_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var activePackage = PackageFactory.Create();
        var inactivePackage = PackageFactory.CreateInactive();
        context.Packages.AddRange(activePackage, inactivePackage);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var packagesProp = doc.RootElement.GetProperty("packages");
        var items = packagesProp.GetProperty("items");

        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeTrue();
        }
    }

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
