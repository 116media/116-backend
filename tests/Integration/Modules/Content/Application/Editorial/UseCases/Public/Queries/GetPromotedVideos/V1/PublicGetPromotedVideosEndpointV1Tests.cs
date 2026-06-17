using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos.V1;

/// <summary>
/// Integration tests for the PublicGetPromotedVideos endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPromotedVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string PromotedUrl => $"{ApiRoutes.Public.Videos}/{EditorialRouteConstants.Promoted}";

    private async Task<(Guid CategoryId, Guid PromotionLevelId)> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, (Guid, Guid)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create();
            ctx.PromotionLevels.Add(promotionLevel);

            return (category.Id, promotionLevel.Id);
        });
    }

    [Fact]
    public async Task GetPromotedVideos_AsAnonymous_ReturnsOk()
    {
        (Guid categoryId, Guid promotionLevelId) = await SeedCategoryAsync();
        VideoEntity promotedVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreatePromoted(categoryId, promotionLevelId);
            ctx.Videos.Add(entity);
            return entity;
        });
        VideoEntity publishedVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(PromotedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPromotedVideosResponse body = await response.ReadAsAsync<PublicGetPromotedVideosResponse>();
        body.Videos.Should().Contain(item => item.Id == promotedVideo.Id);
        body.Videos.Should().NotContain(item => item.Id == publishedVideo.Id);
        body.Videos.Should().OnlyContain(item => item.IsPromoted);
    }
}
