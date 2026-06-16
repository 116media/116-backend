using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory.V1;

/// <summary>
/// Integration tests for the AdminDeactivateCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivateCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<CategoryEntity> SeedCategoryAsync(bool active)
    {
        return await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = active
                ? CategoryFactory.Create(contentType.Id)
                : CategoryFactory.CreateInactive(contentType.Id);
            ctx.Categories.Add(category);
            return category;
        });
    }

    private async Task<bool> IsCategoryActiveAsync(Guid id)
    {
        await using var ctx = CreateDbContext<ContentDbContext>();
        CategoryEntity? category = await ctx.Categories.FindAsync(id);
        return category!.IsActive;
    }

    [Fact]
    public async Task DeactivateCategory_AsSuperAdmin_ReturnsOk()
    {
        CategoryEntity category = await SeedCategoryAsync(active: true);

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Deactivate(category.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsCategoryActiveAsync(category.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateCategory_AsAdmin_ReturnsOk()
    {
        CategoryEntity category = await SeedCategoryAsync(active: true);

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Deactivate(category.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsCategoryActiveAsync(category.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateCategory_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Deactivate(Guid.NewGuid()), null);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Deactivate(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that deactivating a category that is already inactive
    /// returns a 409 Conflict response.
    /// </summary>
    [Fact]
    public async Task DeactivateCategory_WhenAlreadyInactive_ReturnsConflict()
    {
        CategoryEntity category = await SeedCategoryAsync(active: false);

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Deactivate(category.Id), null);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
        (await IsCategoryActiveAsync(category.Id)).Should().BeFalse();
    }
}
