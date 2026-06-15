using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo.V1;

/// <summary>
/// Integration tests for the PublicUnbookmarkShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnbookmarkShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UnbookmarkShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnbookmarkShortVideo_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that unbookmarking a short video that was never bookmarked returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task UnbookmarkShortVideo_WhenNotBookmarked_ReturnsBadRequest()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        context.ShortVideos.Add(shortVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Id}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
