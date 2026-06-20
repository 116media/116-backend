namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle.V1;

/// <summary>
/// Integration tests for the AdminApproveArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminApproveArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ApproveArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
