using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.V1;

/// <summary>
/// Integration tests for the PublicGetVideoFeed endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetVideoFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string FeedUrl => $"{ApiRoutes.Public.Videos}/{EditorialRouteConstants.Feed}";

    [Fact]
    public async Task GetFeed_AsAnonymous_WhenNoPinnedCategories_ReturnsOkEmpty()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
        body.Sections.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeed_AsAnonymous_WithPinnedCategory_ReturnsSection()
    {
        CategoryEntity category = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            category = CategoryFactory.CreatePinned(type.Id);
            ctx.Categories.Add(category);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(category.Id, 5));
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
        body.Sections.Should().ContainSingle(s => s.Category.Id == category.Id);
        body.Sections[0].Videos.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetFeed_ShouldCapVideosAtEight()
    {
        CategoryEntity category = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            category = CategoryFactory.CreatePinned(type.Id);
            ctx.Categories.Add(category);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(category.Id, 12));
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
        body.Sections.Should().ContainSingle();
        body.Sections[0].Videos.Should().HaveCount(EditorialFeedConstants.MaxVideosPerFeedSection);
    }

    [Fact]
    public async Task GetFeed_ShouldOmitCategoryWithNoPublishedVideos()
    {
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            CategoryEntity category = CategoryFactory.CreatePinned(type.Id);
            ctx.Categories.Add(category);
            ctx.Videos.AddRange(VideoFactory.CreateMany(category.Id, 3)); // drafts, not published
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
        body.Sections.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFeed_ShouldOnlyIncludeVideoCategories()
    {
        CategoryEntity videoCategory = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity videoType = ContentTypeFactory.Create("Video");
            ContentTypeEntity articleType = ContentTypeFactory.Create("Article");
            ctx.ContentTypes.AddRange(videoType, articleType);

            videoCategory = CategoryFactory.CreatePinned(videoType.Id);
            ctx.Categories.Add(videoCategory);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(videoCategory.Id, 4));

            CategoryEntity articleCategory = CategoryFactory.CreatePinned(articleType.Id);
            ctx.Categories.Add(articleCategory);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
        body.Sections.Should().ContainSingle(s => s.Category.Id == videoCategory.Id);
    }

    [Fact]
    public async Task GetFeed_WithPinnedCategoryPoster_ResolvesPosterUrl()
    {
        const string posterUrl = "https://cdn.116.test/posters/show.jpg";

        FileEntity poster = await SeedAsync<CoreDbContext, FileEntity>(ctx =>
        {
            FileEntity file = FileFactory.CreateWithStorageUrl(posterUrl);
            ctx.Files.Add(file);
            return file;
        });

        CategoryEntity category = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            category = CategoryFactory.CreatePinnedWithPoster(type.Id, poster.Id);
            ctx.Categories.Add(category);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(category.Id, 4));
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
        VideoFeedSectionDto section = body.Sections.Single(s => s.Category.Id == category.Id);
        section.Category.PosterUrl.Should().Be(posterUrl);
    }

    [Fact]
    public async Task GetFeed_ShouldOrderSectionsByMostRecentlyPinned()
    {
        CategoryEntity older = null!;
        CategoryEntity newer = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);

            older = CategoryFactory.CreatePinned(type.Id, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
            newer = CategoryFactory.CreatePinned(type.Id, new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));
            ctx.Categories.AddRange(older, newer);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(older.Id, 4));
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(newer.Id, 4));
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
        body.Sections.Should().HaveCount(2);
        body.Sections[0].Category.Id.Should().Be(newer.Id);
        body.Sections[1].Category.Id.Should().Be(older.Id);
    }
}
