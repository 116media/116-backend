using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Content.Api.Catalog;

/// <summary>
/// Integration tests for the category query endpoints:
/// <c>GET /api/v1/admin/categories</c> (AdminGetAllCategories),
/// <c>GET /api/v1/admin/categories/{id}</c> (AdminGetCategoryById),
/// <c>GET /api/v1/public/categories</c> (PublicGetActiveCategories),
/// and <c>GET /api/v1/public/categories/exclusive</c> (PublicGetExclusiveCategory).
/// </summary>
[Collection("Database")]
public class CategoryQueryEndpointTests(PostgresFixture db) : BaseApiTest(db)
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

    [Fact]
    public async Task AdminGetCategoryById_WithoutAuthentication_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetCategoryById_AsVisitor_ReturnsForbidden()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}/{category.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminGetCategoryById_AsSuperAdmin_WithExistingCategory_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}/{category.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var categoryProp = doc.RootElement.GetProperty("category");

        categoryProp.GetProperty("id").GetString().Should().Be(category.Id.ToString());
    }

    [Fact]
    public async Task AdminGetCategoryById_WithNonExistentGuid_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PublicGetActiveCategories_AsAnonymous_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var activeCategory = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(activeCategory);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Categories);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        doc.RootElement.TryGetProperty("categories", out var categoriesProp).Should().BeTrue();
        categoriesProp.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task PublicGetActiveCategories_WithContentTypeFilter_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType1 = ContentTypeFactory.Create();
        var contentType2 = ContentTypeFactory.Create();
        context.ContentTypes.AddRange(contentType1, contentType2);
        await context.SaveChangesAsync();

        var category1 = CategoryFactory.Create(contentType1.Id);
        var category2 = CategoryFactory.Create(contentType2.Id);
        context.Categories.AddRange(category1, category2);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Categories}?contentTypeId={contentType1.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var categoriesProp = doc.RootElement.GetProperty("categories");

        categoriesProp.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task PublicGetExclusiveCategory_AsAnonymous_WhenNoExclusive_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Categories}/exclusive?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PublicGetExclusiveCategory_AsAnonymous_WithSeededExclusive_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var exclusiveCategory = CategoryFactory.Create(contentType.Id, isExclusive: true);
        context.Categories.Add(exclusiveCategory);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Categories}/exclusive?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("category", out var categoryProp).Should().BeTrue();
        categoryProp.GetProperty("id").GetString().Should().Be(exclusiveCategory.Id.ToString());
    }
}
