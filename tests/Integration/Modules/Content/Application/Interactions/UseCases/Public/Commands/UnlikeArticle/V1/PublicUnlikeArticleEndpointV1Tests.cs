namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle.V1;

/// <summary>
/// Integration tests for the PublicUnlikeArticle endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnlikeArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UnlikeArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/likes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlikeArticle_AsVisitor_NonExistentLike_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/likes");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
