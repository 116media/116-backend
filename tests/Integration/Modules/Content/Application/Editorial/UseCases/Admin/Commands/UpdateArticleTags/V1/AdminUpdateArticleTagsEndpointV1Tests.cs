namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags.V1;

/// <summary>
/// Integration tests for the AdminUpdateArticleTags endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateArticleTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateArticleTags_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/tags", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateArticleTags_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/tags", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateArticleTags_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/tags", new { });

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }
}
