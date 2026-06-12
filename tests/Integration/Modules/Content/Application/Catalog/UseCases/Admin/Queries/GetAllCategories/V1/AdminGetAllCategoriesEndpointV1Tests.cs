using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories.V1;

/// <summary>
/// Integration tests for the AdminGetAllCategories endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllCategoriesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminGetAllCategories_WithoutAuthentication_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetAllCategories_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminGetAllCategories_AsSuperAdmin_ReturnsOkWithItems()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var categories = CategoryFactory.CreateMany(contentType.Id, 3);
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("categories", out var categoriesProp).Should().BeTrue();
        categoriesProp.TryGetProperty("items", out var items).Should().BeTrue();
        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task AdminGetAllCategories_WithIsActiveFilter_ReturnsOnlyActiveCategories()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var activeCategory = CategoryFactory.Create(contentType.Id);
        var inactiveCategory = CategoryFactory.CreateInactive(contentType.Id);
        context.Categories.AddRange(activeCategory, inactiveCategory);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var categoriesProp = doc.RootElement.GetProperty("categories");
        var items = categoriesProp.GetProperty("items");

        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isActive").GetBoolean().Should().BeTrue();
        }
    }

    [Fact]
    public async Task AdminGetAllCategories_WithIsFreeFilter_ReturnsOnlyFreeCategories()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var freeCategory = CategoryFactory.CreateFree(contentType.Id);
        var paidCategory = CategoryFactory.CreatePaid(contentType.Id);
        context.Categories.AddRange(freeCategory, paidCategory);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}?pageIndex=0&pageSize=50&isFree=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var categoriesProp = doc.RootElement.GetProperty("categories");
        var items = categoriesProp.GetProperty("items");

        for (var i = 0; i < items.GetArrayLength(); i++)
        {
            items[i].GetProperty("isFree").GetBoolean().Should().BeTrue();
        }
    }
}
