namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkArticle.V1;

/// <summary>
/// Integration tests for the PublicUnbookmarkArticle endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnbookmarkArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UnbookmarkArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnbookmarkArticle_AsVisitor_NonExistentBookmark_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/bookmarks");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
