namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle.V1;

/// <summary>
/// Integration tests for the AdminPublishArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminPublishArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublishArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublishArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
