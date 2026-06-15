using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory.V1;

/// <summary>
/// Integration tests for the AdminDeactivateCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivateCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task DeactivateCategory_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivateCategory_AsAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivateCategory_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that deactivating a category that is already inactive
    /// returns a 409 Conflict response.
    /// </summary>
    [Fact]
    public async Task DeactivateCategory_WhenAlreadyInactive_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.CreateInactive(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
