using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
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

        IEnumerable<VideoSummaryDto> allFeedVideos = body
            .Spot1.Videos.Concat(body.Spot2.Videos)
            .Concat(body.Spot3.Slots.SelectMany(slot => slot.Videos))
            .Concat(body.FreeVideoStrip);

        allFeedVideos.Should().Contain(item => item.Id == freeVideo.Id);
    }
}
