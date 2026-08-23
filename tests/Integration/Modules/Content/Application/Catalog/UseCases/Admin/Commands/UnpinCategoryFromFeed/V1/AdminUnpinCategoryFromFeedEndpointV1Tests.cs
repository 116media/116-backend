using _116.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed.V1;

/// <summary>
/// Integration tests for the AdminUnpinCategoryFromFeed endpoint.
/// </summary>
[Collection("Database")]
public class AdminUnpinCategoryFromFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string UnpinSegment = "unpin-from-feed";

    private async Task<DateTimeOffset?> PinnedAtAsync(Guid id)
    {
        await using var ctx = CreateDbContext<ContentDbContext>();
        CategoryEntity? category = await ctx.Categories.FindAsync(id);
        return category!.PinnedToFeedAt;
    }

    [Fact]
    public async Task Unpin_AsSuperAdmin_WhenPinned_ReturnsOk()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            CategoryEntity cat = CategoryFactory.CreatePinned(type.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{UnpinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUnpinCategoryFromFeedResponse>();
        body.Category.IsPinnedToFeed.Should().BeFalse();

        (await PinnedAtAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Unpin_AsSuperAdmin_WhenNotPinned_ReturnsOk()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            CategoryEntity cat = CategoryFactory.Create(type.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{UnpinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await PinnedAtAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Unpin_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{UnpinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Unpin_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{UnpinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unpin_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{UnpinSegment}", null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }
}
