using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById.V1;

/// <summary>
/// Integration tests for the AdminGetCategoryById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetCategoryByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<CategoryEntity> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            return category;
        });
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
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}/{category.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminGetCategoryById_AsSuperAdmin_WithExistingCategory_ReturnsOk()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}/{category.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetCategoryByIdResponse>();
        body.Category.Id.Should().Be(category.Id);
        body.Category.Name.Should().Be(category.Name);
        body.Category.Slug.Should().Be(category.Slug);
    }

    [Fact]
    public async Task AdminGetCategoryById_WithNonExistentGuid_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }
}
