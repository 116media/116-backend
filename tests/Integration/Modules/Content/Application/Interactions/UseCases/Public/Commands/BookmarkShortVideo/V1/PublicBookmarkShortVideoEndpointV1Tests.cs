using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo.V1;

/// <summary>
/// Integration tests for the PublicBookmarkShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicBookmarkShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task BookmarkShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/bookmarks", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BookmarkShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/bookmarks", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that bookmarking a short video that is already bookmarked returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task BookmarkShortVideo_WhenAlreadyBookmarked_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        context.ShortVideos.Add(shortVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Id}/bookmarks", null);

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Id}/bookmarks", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
