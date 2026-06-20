using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.V1;

/// <summary>
/// Integration tests for the PublicGetExclusiveCategory endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetExclusiveCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
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
