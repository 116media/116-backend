namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment.V1;

/// <summary>
/// Integration tests for the PublicDeleteArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class PublicDeleteArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeleteArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteArticleComment_AsVisitor_NonExistentComment_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
