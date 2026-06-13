using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories.V1;

/// <summary>
/// Integration tests for the PublicGetActiveCategories endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetActiveCategoriesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
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
}
