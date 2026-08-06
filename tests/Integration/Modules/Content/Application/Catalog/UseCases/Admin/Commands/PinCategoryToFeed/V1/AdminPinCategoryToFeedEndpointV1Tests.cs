using _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed.V1;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed.V1;

/// <summary>
/// Integration tests for the AdminPinCategoryToFeed endpoint.
/// </summary>
[Collection("Database")]
public class AdminPinCategoryToFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string PinSegment = "pin-to-feed";

    private async Task<DateTimeOffset?> PinnedAtAsync(Guid id)
    {
        await using var ctx = CreateDbContext<ContentDbContext>();
        CategoryEntity? category = await ctx.Categories.FindAsync(id);
        return category!.PinnedToFeedAt;
    }

    [Fact]
    public async Task Pin_AsSuperAdmin_WhenEligible_ReturnsOk()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            CategoryEntity cat = CategoryFactory.Create(type.Id);
            ctx.Categories.Add(cat);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(cat.Id, 4));
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{PinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminPinCategoryToFeedResponse>();
        body.Category.Id.Should().Be(category.Id);
        body.Category.IsPinnedToFeed.Should().BeTrue();

        (await PinnedAtAsync(category.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Pin_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{PinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Pin_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{PinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Pin_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{PinSegment}", null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task Pin_WhenInactive_ReturnsBadRequest()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            CategoryEntity cat = CategoryFactory.CreateInactive(type.Id);
            ctx.Categories.Add(cat);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(cat.Id, 4));
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{PinSegment}", null);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<CategoryErrorMessage>(m => m.CannotPinInactiveToFeed())
        );
        (await PinnedAtAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Pin_WhenNonVideoCategory_ReturnsBadRequest()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Article");
            ctx.ContentTypes.Add(type);
            CategoryEntity cat = CategoryFactory.Create(type.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{PinSegment}", null);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<CategoryErrorMessage>(m => m.ContentTypeNotFeedable())
        );
        (await PinnedAtAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Pin_WhenFewerThanMinimumPublishedVideos_ReturnsBadRequest()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            CategoryEntity cat = CategoryFactory.Create(type.Id);
            ctx.Categories.Add(cat);
            ctx.Videos.AddRange(
                VideoFactory.CreateManyPublished(cat.Id, EditorialFeedConstants.MinVideosToPinToFeed - 1)
            );
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{PinSegment}", null);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<CategoryErrorMessage>(m =>
                m.NotEnoughVideosToPinToFeed(EditorialFeedConstants.MinVideosToPinToFeed)
            )
        );
        (await PinnedAtAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Pin_WhenCapReached_EvictsOldest()
    {
        var oldestId = Guid.Empty;
        CategoryEntity newCategory = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);

            var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            List<CategoryEntity> pinned = CategoryFactory.CreateManyPinned(type.Id, 5, baseTime);
            oldestId = pinned[0].Id;
            ctx.Categories.AddRange(pinned);

            CategoryEntity fresh = CategoryFactory.Create(type.Id);
            ctx.Categories.Add(fresh);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(fresh.Id, 4));
            return fresh;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{newCategory.Id}/{PinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await PinnedAtAsync(oldestId)).Should().BeNull();
        (await PinnedAtAsync(newCategory.Id)).Should().NotBeNull();

        await using var verifyCtx = CreateDbContext<ContentDbContext>();
        int pinnedCount = verifyCtx.Categories.Count(c => c.PinnedToFeedAt != null);
        pinnedCount.Should().Be(5);
    }
}
