using _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories.V1;
using _116.Content.Domain.Entities;
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
        CategoryEntity activeCategory = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            activeCategory = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(activeCategory);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Categories);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetActiveCategoriesResponse>();
        body.Categories.Should().Contain(c => c.Id == activeCategory.Id);
        body.Categories.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task PublicGetActiveCategories_WithContentTypeFilter_ReturnsFilteredResults()
    {
        ContentTypeEntity contentType1 = null!;
        CategoryEntity category1 = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            contentType1 = ContentTypeFactory.Create();
            ContentTypeEntity contentType2 = ContentTypeFactory.Create();
            ctx.ContentTypes.AddRange(contentType1, contentType2);
            category1 = CategoryFactory.Create(contentType1.Id);
            CategoryEntity category2 = CategoryFactory.Create(contentType2.Id);
            ctx.Categories.AddRange(category1, category2);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Categories}?contentTypeId={contentType1.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetActiveCategoriesResponse>();
        body.Categories.Should().Contain(c => c.Id == category1.Id);
        body.Categories.Should().OnlyContain(c => c.ContentTypeId == contentType1.Id);
    }
}
