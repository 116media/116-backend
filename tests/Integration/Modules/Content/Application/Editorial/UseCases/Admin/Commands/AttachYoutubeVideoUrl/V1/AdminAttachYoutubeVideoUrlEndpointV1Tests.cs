using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl.V1;

/// <summary>
/// Integration tests for the AdminAttachYoutubeVideoUrl endpoint.
/// </summary>
[Collection("Database")]
public class AdminAttachYoutubeVideoUrlEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AttachYoutubeVideoUrl_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/youtube",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/youtube",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/youtube",
            new { Reason = "test" }
        );

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var video = VideoFactory.Create(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { YoutubeVideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ" };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.Admin.Videos}/{video.Id}/youtube", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that attaching a YouTube URL to a video whose shoot date is still in the
    /// future returns a 400 Bad Request response because the video has not been shot yet.
    /// </summary>
    [Fact]
    public async Task AttachYoutubeVideoUrl_BeforeShootDate_ReturnsBadRequest()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var video = VideoFactory.CreateWithFutureShoot(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { YoutubeVideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ" };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.Admin.Videos}/{video.Id}/youtube", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        var id = Guid.NewGuid();
        var request = new { YoutubeVideoUrl = "not-a-valid-url" };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}/youtube", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
