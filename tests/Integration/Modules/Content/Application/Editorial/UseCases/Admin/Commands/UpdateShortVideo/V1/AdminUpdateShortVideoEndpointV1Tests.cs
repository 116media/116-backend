using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminUpdateShortVideo endpoint.
/// Metadata is updated via JSON; the video file is replaced separately.
/// </summary>
[Collection("Database")]
public class AdminUpdateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static object BuildRequest(string title, Guid? videoId = null) => new { Title = title, VideoId = videoId };

    [Fact]
    public async Task UpdateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Shorts}/{nonExistentId}",
            BuildRequest("Updated Title")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Shorts}/{nonExistentId}",
            BuildRequest("Updated Title")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateShortVideo_AsAdmin_WhenNotFound_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Shorts}/{nonExistentId}",
            BuildRequest("Updated Title")
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateShortVideo_WithInvalidGuid_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Shorts}/not-a-guid",
            BuildRequest("Updated Title")
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateShortVideo_WithEmptyTitle_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Shorts}/{id}", BuildRequest(string.Empty));

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating the title of an existing short video succeeds, echoes the new
    /// title in the response DTO, and persists it.
    /// </summary>
    [Fact]
    public async Task UpdateShortVideo_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Shorts}/{shortVideo.Id}",
            BuildRequest("Updated Short Video Title")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateShortVideoResponse body = await response.ReadAsAsync<AdminUpdateShortVideoResponse>();
        body.ShortVideo.Id.Should().Be(shortVideo.Id);
        body.ShortVideo.Title.Should().Be("Updated Short Video Title");

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? persisted = await verifyContext.ShortVideos.FindAsync(shortVideo.Id);
        persisted!.Title.Should().Be("Updated Short Video Title");
    }

    /// <summary>
    /// Verifies that updating a short video with a title exceeding the maximum allowed length
    /// returns a 400 Bad Request response from the ValidShortVideoTitle validator rule.
    /// </summary>
    [Fact]
    public async Task UpdateShortVideo_WithTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Shorts}/{id}",
            BuildRequest(new string('T', 300))
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
