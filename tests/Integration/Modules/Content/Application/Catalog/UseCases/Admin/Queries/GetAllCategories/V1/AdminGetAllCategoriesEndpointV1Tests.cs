using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories.V1;
using _116.Content.Domain.Entities;
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
        List<CategoryEntity> categories = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            categories = CategoryFactory.CreateMany(contentType.Id, 3);
            ctx.Categories.AddRange(categories);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllCategoriesResponse>();
        body.Categories.PageIndex.Should().Be(0);
        body.Categories.PageSize.Should().Be(10);
        body.Categories.Count.Should().Be(3);
        body.Categories.Items.Should().Contain(c => c.Id == categories[0].Id);
    }

    [Fact]
    public async Task AdminGetAllCategories_WithIsActiveFilter_ReturnsOnlyActiveCategories()
    {
        CategoryEntity activeCategory = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            activeCategory = CategoryFactory.Create(contentType.Id);
            CategoryEntity inactiveCategory = CategoryFactory.CreateInactive(contentType.Id);
            ctx.Categories.AddRange(activeCategory, inactiveCategory);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}?pageIndex=0&pageSize=50&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllCategoriesResponse>();
        body.Categories.Items.Should().OnlyContain(c => c.IsActive);
        body.Categories.Items.Should().Contain(c => c.Id == activeCategory.Id);
    }

    [Fact]
    public async Task AdminGetAllCategories_WithIsFreeFilter_ReturnsOnlyFreeCategories()
    {
        CategoryEntity freeCategory = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            freeCategory = CategoryFactory.CreateFree(contentType.Id);
            CategoryEntity paidCategory = CategoryFactory.CreatePaid(contentType.Id);
            ctx.Categories.AddRange(freeCategory, paidCategory);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}?pageIndex=0&pageSize=50&isFree=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllCategoriesResponse>();
        body.Categories.Items.Should().OnlyContain(c => c.IsFree);
        body.Categories.Items.Should().Contain(c => c.Id == freeCategory.Id);
    }
}
