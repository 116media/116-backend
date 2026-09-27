using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Content.Integration.Tests.Application.Catalog.UseCases.Admin.Commands.ActivateCategory.V1;

/// <summary>
/// Integration tests for the AdminActivateCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminActivateCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<CategoryEntity> SeedCategoryAsync(bool active)
    {
        return await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            var contentType = ContentTypeFactory.Create();
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
    public async Task ActivateCategory_AsSuperAdmin_ActivatesAndPersists()
    {
        CategoryEntity category = await SeedCategoryAsync(active: false);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Activate(category.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsCategoryActiveAsync(category.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ActivateCategory_AsAdmin_ActivatesAndPersists()
    {
        CategoryEntity category = await SeedCategoryAsync(active: false);
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Activate(category.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsCategoryActiveAsync(category.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ActivateCategory_NonExistentCategory_ReturnsNotFoundProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Activate(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task ActivateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Activate(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActivateCategory_WhenAlreadyActive_ReturnsConflictProblem()
    {
        CategoryEntity category = await SeedCategoryAsync(active: true);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Categories.Activate(category.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<CategoryErrorMessage>(m => m.AlreadyActive())
        );
        (await IsCategoryActiveAsync(category.Id)).Should().BeTrue();
    }
}
