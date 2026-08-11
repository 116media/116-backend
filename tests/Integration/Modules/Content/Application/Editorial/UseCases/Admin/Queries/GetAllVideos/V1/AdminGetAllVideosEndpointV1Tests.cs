using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideos.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideos.V1;

/// <summary>
/// Integration tests for the AdminGetAllVideos endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetAllVideos_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Videos);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllVideos_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.Videos);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllVideos_AsAdmin_IsAllowed()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Videos);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllVideosResponse body = await response.ReadAsAsync<AdminGetAllVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == video.Id);
        body.Videos.Count.Should().Be(1);
        body.Videos.PageIndex.Should().Be(0);
        body.Videos.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllVideos_WithSearch_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllVideosResponse body = await response.ReadAsAsync<AdminGetAllVideosResponse>();
        body.Videos.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllVideos_WithStatusFilter_ReturnsFilteredResults()
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

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}?status={nameof(EnumContentStatus.Published)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllVideosResponse body = await response.ReadAsAsync<AdminGetAllVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == publishedVideo.Id);
        body.Videos.Items.Should().NotContain(item => item.Id == draftVideo.Id);
        body.Videos.Items.Should().OnlyContain(item => item.Status == EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetAllVideos_WithSearchQuery_ReturnsFilteredResults()
    {
        Guid categoryId = await SeedCategoryAsync();

        VideoEntity matchingVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreateWithTitle(categoryId, "UniqueSearchTerm Video");
            ctx.Videos.Add(entity);
            return entity;
        });
        VideoEntity otherVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.CreateWithTitle(categoryId, "Completely Different Title");
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}?search=UniqueSearchTerm");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllVideosResponse body = await response.ReadAsAsync<AdminGetAllVideosResponse>();
        body.Videos.Items.Should().Contain(item => item.Id == matchingVideo.Id);
        body.Videos.Items.Should().NotContain(item => item.Id == otherVideo.Id);
        body.Videos.Items.Should().OnlyContain(item => item.Title.Contains("UniqueSearchTerm"));
    }
}
