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
}
