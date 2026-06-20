namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks.V1;

/// <summary>
/// Integration tests for the PublicGetMyArticleBookmarks endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetMyArticleBookmarksEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetMyArticleBookmarks_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyArticleBookmarks_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMyArticleBookmarks_AsVisitor_WithPagination_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/bookmarks?pageIndex=0&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
