using _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo.V1;

/// <summary>
/// Integration tests for the PublicUnbookmarkShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnbookmarkShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ShortVideoEntity> SeedShortVideoAsync()
    {
        return await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity shortVideo = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(shortVideo);
            return shortVideo;
        });
    }

    [Fact]
    public async Task UnbookmarkShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Public.Shorts.Bookmarks(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnbookmarkShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Shorts.Bookmarks(Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that unbookmarking a previously bookmarked short video removes the bookmark row.
    /// </summary>
    [Fact]
    public async Task UnbookmarkShortVideo_WhenBookmarked_RemovesBookmarkAndPersists()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Shorts.Bookmarks(shortVideo.Id), null);

        var response = await Client.DeleteAsync(Routes.Public.Shorts.Bookmarks(shortVideo.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicUnbookmarkShortVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (
            await verifyDb.ShortVideoBookmarks.AnyAsync(b =>
                b.ShortVideoId == shortVideo.Id && b.UserId == TestUser.VisitorId
            )
        )
            .Should()
            .BeFalse();
    }

    /// <summary>
    /// Verifies that unbookmarking a short video that was never bookmarked returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task UnbookmarkShortVideo_WhenNotBookmarked_ReturnsBadRequest()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Shorts.Bookmarks(shortVideo.Id));

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
