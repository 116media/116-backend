using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl.V1;

/// <summary>
/// Integration tests for the AdminAttachYoutubeVideoUrl endpoint.
/// </summary>
[Collection("Database")]
public class AdminAttachYoutubeVideoUrlEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string ValidYoutubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

    private async Task<VideoEntity> SeedVideoAsync(Func<Guid, VideoEntity> create)
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity video = create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(video);
            return video;
        });
    }

    private async Task<VideoEntity> GetVideoAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? video = await ctx.Videos.FindAsync(id);
        return video!;
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { YoutubeVideoUrl = ValidYoutubeUrl }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { YoutubeVideoUrl = ValidYoutubeUrl }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        AdminAttachYoutubeVideoUrlRequest request = new AdminAttachYoutubeVideoUrlRequestBuilder()
            .WithYoutubeVideoUrl(ValidYoutubeUrl)
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that attaching a valid YouTube URL to a draft video (with no future shoot
    /// scheduled) persists the URL and resolves a thumbnail via the stubbed services.
    /// </summary>
    [Fact]
    public async Task AttachYoutubeVideoUrl_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.Create(categoryId));
        Client.AuthenticateAsSuperAdmin();
        AdminAttachYoutubeVideoUrlRequest request = new AdminAttachYoutubeVideoUrlRequestBuilder()
            .WithYoutubeVideoUrl(ValidYoutubeUrl)
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, video.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminAttachYoutubeVideoUrlResponse body = await response.ReadAsAsync<AdminAttachYoutubeVideoUrlResponse>();
        body.Video.Id.Should().Be(video.Id);
        body.Video.YoutubeVideoUrl.Should().Be(request.YoutubeVideoUrl);

        VideoEntity persisted = await GetVideoAsync(video.Id);
        persisted.YoutubeVideoUrl.Should().Be(request.YoutubeVideoUrl);
        persisted.ThumbnailFileId.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that attaching a YouTube URL to a video whose shoot date is still in the
    /// future returns a 400 Bad Request response because the video has not been shot yet.
    /// </summary>
    [Fact]
    public async Task AttachYoutubeVideoUrl_BeforeShootDate_ReturnsBadRequest()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.CreateWithFutureShoot(categoryId));
        Client.AuthenticateAsSuperAdmin();
        AdminAttachYoutubeVideoUrlRequest request = new AdminAttachYoutubeVideoUrlRequestBuilder()
            .WithYoutubeVideoUrl(ValidYoutubeUrl)
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, video.Id),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
        (await GetVideoAsync(video.Id)).YoutubeVideoUrl.Should().BeNull();
    }

    /// <summary>
    /// Verifies that attaching an invalid YouTube URL (not matching youtube.com/watch,
    /// youtu.be, youtube.com/embed, or youtube.com/shorts patterns) returns a
    /// 400 Bad Request response from the validator.
    /// </summary>
    [Fact]
    public async Task AttachYoutubeVideoUrl_WithInvalidYoutubeUrl_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        AdminAttachYoutubeVideoUrlRequest request = new AdminAttachYoutubeVideoUrlRequestBuilder()
            .WithYoutubeVideoUrl("not-a-valid-url")
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, id),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
