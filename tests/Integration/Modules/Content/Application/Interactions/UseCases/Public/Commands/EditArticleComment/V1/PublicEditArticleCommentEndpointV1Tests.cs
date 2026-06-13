namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment.V1;

/// <summary>
/// Integration tests for the PublicEditArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class PublicEditArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task EditArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Body = "Updated comment body." };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EditArticleComment_AsVisitor_NonExistentComment_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Body = "Updated comment body." };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
