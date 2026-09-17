using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed.V1;

/// <summary>
/// Integration tests for the PublicGetVideoPromotionFeed endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetVideoPromotionFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string FeedUrl => $"{ApiRoutes.Public.Videos}/{EditorialRouteConstants.PromotionFeed}";

    [Fact]
    public async Task GetVideoPromotionFeed_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetVideoPromotionFeedResponse body = await response.ReadAsAsync<PublicGetVideoPromotionFeedResponse>();
        body.Spot1.SpotPriority.Should().Be(EditorialFeedConstants.Spot1);
        body.Spot2.SpotPriority.Should().Be(EditorialFeedConstants.Spot2);
        body.Spot3.SpotPriority.Should().Be(EditorialFeedConstants.Spot3);
    }

    [Fact]
    public async Task GetVideoPromotionFeed_WithFreeVideo_IncludesVideoInFeed()
    {
        Guid categoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });

        VideoEntity freeVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetVideoPromotionFeedResponse body = await response.ReadAsAsync<PublicGetVideoPromotionFeedResponse>();

        IEnumerable<PublicVideoSummaryDto> allFeedVideos = body
            .Spot1.Videos.Concat(body.Spot2.Videos)
            .Concat(body.Spot3.Slots.SelectMany(slot => slot.Videos))
            .Concat(body.FreeVideoStrip);

        allFeedVideos.Should().Contain(item => item.Id == freeVideo.Id);
    }

    /// <summary>
    /// Seeds a content type and category, returning the category id every video hangs off.
    /// </summary>
    /// <returns>The seeded category id.</returns>
    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });
    }

    /// <summary>
    /// Seeds a published, promoted video assigned to the given spot.
    /// </summary>
    /// <param name="categoryId">The category the video belongs to.</param>
    /// <param name="spotPriority">The spot the promotion level targets.</param>
    /// <returns>The seeded video.</returns>
    private async Task<VideoEntity> SeedPromotedVideoAsync(Guid categoryId, int spotPriority)
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            PromotionLevelEntity level = new PromotionLevelBuilder().WithSpotPriority(spotPriority).Build();
            ctx.PromotionLevels.Add(level);

            VideoEntity video = VideoFactory.CreatePromoted(categoryId, level.Id);
            ctx.Videos.Add(video);

            return video;
        });
    }

    [Fact]
    public async Task GetVideoPromotionFeed_WithPromotedVideos_PlacesThemInTheirOwnSpots()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity spot1Video = await SeedPromotedVideoAsync(categoryId, EditorialFeedConstants.Spot1);
        VideoEntity spot2Video = await SeedPromotedVideoAsync(categoryId, EditorialFeedConstants.Spot2);

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetVideoPromotionFeedResponse body = await response.ReadAsAsync<PublicGetVideoPromotionFeedResponse>();
        body.Spot1.Videos.Should().ContainSingle(video => video.Id == spot1Video.Id);
        body.Spot2.Videos.Should().ContainSingle(video => video.Id == spot2Video.Id);
    }

    [Fact]
    public async Task GetVideoPromotionFeed_WithTwoSpot3Videos_SplitsThemAcrossBothColumns()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity first = await SeedPromotedVideoAsync(categoryId, EditorialFeedConstants.Spot3);
        VideoEntity second = await SeedPromotedVideoAsync(categoryId, EditorialFeedConstants.Spot3);

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetVideoPromotionFeedResponse body = await response.ReadAsAsync<PublicGetVideoPromotionFeedResponse>();
        body.Spot3.Slots.Should().HaveCount(2);

        // The round-robin puts one promoted video in each column, so neither falls back to a free video.
        IReadOnlyList<Guid> placed = [.. body.Spot3.Slots.SelectMany(slot => slot.Videos).Select(video => video.Id)];
        placed.Should().Contain([first.Id, second.Id]);
        body.Spot3.Slots.Should().AllSatisfy(slot => slot.Videos.Should().ContainSingle());
    }

    [Fact]
    public async Task GetVideoPromotionFeed_WithNoPromotedVideos_FallsBackToFreeVideos()
    {
        Guid categoryId = await SeedCategoryAsync();

        // Four videos fill the spot fallbacks; the rest are what the strip can still draw on.
        const int spotFallbacks = 4;
        int seeded = spotFallbacks + EditorialFeedConstants.DefaultStripSize;

        await SeedAsync<ContentDbContext>(ctx =>
        {
            for (var index = 0; index < seeded; index++)
            {
                ctx.Videos.Add(VideoFactory.CreatePublished(categoryId));
            }
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetVideoPromotionFeedResponse body = await response.ReadAsAsync<PublicGetVideoPromotionFeedResponse>();

        // Every spot borrows one free video, and no video is placed twice across the whole feed.
        body.Spot1.Videos.Should().ContainSingle();
        body.Spot2.Videos.Should().ContainSingle();
        body.Spot3.Slots.Should().AllSatisfy(slot => slot.Videos.Should().ContainSingle());

        body.FreeVideoStrip.Should().HaveCount(EditorialFeedConstants.DefaultStripSize);

        IReadOnlyList<Guid> allIds =
        [
            .. body.Spot1.Videos.Concat(body.Spot2.Videos).Select(video => video.Id),
            .. body.Spot3.Slots.SelectMany(slot => slot.Videos).Select(video => video.Id),
            .. body.FreeVideoStrip.Select(video => video.Id),
        ];
        allIds.Should().HaveCount(seeded).And.OnlyHaveUniqueItems();
    }
}
