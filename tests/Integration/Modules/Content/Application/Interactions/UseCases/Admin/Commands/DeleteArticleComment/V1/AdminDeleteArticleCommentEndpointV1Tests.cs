namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment.V1;

/// <summary>
/// Integration tests for the AdminDeleteArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeleteArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteArticleComment_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteArticleComment_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
