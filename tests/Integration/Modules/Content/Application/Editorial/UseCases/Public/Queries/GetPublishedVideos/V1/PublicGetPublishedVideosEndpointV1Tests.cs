using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;

/// <summary>
/// Integration tests for the PublicGetPublishedVideos endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublishedVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
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

    [Fact]
    public async Task GetPublishedVideos_AsAnonymous_ReturnsOk()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity publishedVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });
        VideoEntity draftVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Videos);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await response.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == publishedVideo.Id);
        body.Videos.Items.Should().NotContain(item => item.Id == draftVideo.Id);
        body.Videos.Items.Should().OnlyContain(item => item.Status == EnumContentStatus.Published);
        body.Videos.PageIndex.Should().Be(0);
        body.Videos.PageSize.Should().Be(10);
    }
}
