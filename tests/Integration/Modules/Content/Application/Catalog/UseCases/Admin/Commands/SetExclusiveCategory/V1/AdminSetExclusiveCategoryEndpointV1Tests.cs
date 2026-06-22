using _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.V1;

/// <summary>
/// Integration tests for the AdminSetExclusiveCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminSetExclusiveCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string SetExclusiveSegment = "set-exclusive";

    private async Task<bool> IsCategoryExclusiveAsync(Guid id)
    {
        await using var ctx = CreateDbContext<ContentDbContext>();
        CategoryEntity? category = await ctx.Categories.FindAsync(id);
        return category!.IsExclusive;
    }

    [Fact]
    public async Task SetExclusiveCategory_AsSuperAdmin_ReturnsOk()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(contentType);
            CategoryEntity cat = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Categories}/{category.Id}/{SetExclusiveSegment}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminSetExclusiveCategoryResponse>();
        body.Category.Id.Should().Be(category.Id);
        body.Category.IsExclusive.Should().BeTrue();

        (await IsCategoryExclusiveAsync(category.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task SetExclusiveCategory_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{SetExclusiveSegment}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetExclusiveCategory_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{SetExclusiveSegment}",
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetExclusiveCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{SetExclusiveSegment}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that setting an inactive category as exclusive
    /// returns a 400 BadRequest response.
    /// </summary>
    [Fact]
    public async Task SetExclusive_WhenInactive_ReturnsBadRequest()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(contentType);
            CategoryEntity cat = CategoryFactory.CreateInactive(contentType.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Categories}/{category.Id}/{SetExclusiveSegment}",
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
        (await IsCategoryExclusiveAsync(category.Id)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that setting a non-Video category as exclusive
    /// returns a 400 BadRequest response because only Video categories can be exclusive.
    /// </summary>
    [Fact]
    public async Task SetExclusive_WhenNonVideoCategory_ReturnsBadRequest()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create("Article");
            ctx.ContentTypes.Add(contentType);
            CategoryEntity cat = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Categories}/{category.Id}/{SetExclusiveSegment}",
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
        (await IsCategoryExclusiveAsync(category.Id)).Should().BeFalse();
    }
}
