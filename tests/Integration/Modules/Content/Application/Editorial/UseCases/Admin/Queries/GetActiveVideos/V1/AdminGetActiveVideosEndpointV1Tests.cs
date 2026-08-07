using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos.V1;

/// <summary>
/// Integration tests for the AdminGetActiveVideos endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetActiveVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ActiveUrl => $"{ApiRoutes.Admin.Videos}/{EditorialRouteConstants.Active}";

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
    public async Task GetActiveVideos_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ActiveUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActiveVideos_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ActiveUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetActiveVideos_AsAdmin_IsAllowed()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity activeVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity video = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.Add(video);
            return video;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ActiveUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetActiveVideosResponse body = await response.ReadAsAsync<AdminGetActiveVideosResponse>();
        body.Videos.Should().Contain(video => video.Id == activeVideo.Id);
    }

    [Fact]
    public async Task GetActiveVideos_ExcludesArchivedAndRejectedVideos()
    {
        Guid categoryId = await SeedCategoryAsync();

        VideoEntity publishedVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity video = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.Add(video);
            return video;
        });
        VideoEntity archivedVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity video = VideoFactory.CreateArchived(categoryId);
            ctx.Videos.Add(video);
            return video;
        });
        VideoEntity rejectedVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity video = VideoFactory.CreateRejected(categoryId);
            ctx.Videos.Add(video);
            return video;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(ActiveUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetActiveVideosResponse body = await response.ReadAsAsync<AdminGetActiveVideosResponse>();
        body.Videos.Should().Contain(video => video.Id == publishedVideo.Id);
        body.Videos.Should().NotContain(video => video.Id == archivedVideo.Id);
        body.Videos.Should().NotContain(video => video.Id == rejectedVideo.Id);
        body.Videos.Should()
            .OnlyContain(video =>
                video.Status != EnumContentStatus.Archived && video.Status != EnumContentStatus.Rejected
            );
    }
}
