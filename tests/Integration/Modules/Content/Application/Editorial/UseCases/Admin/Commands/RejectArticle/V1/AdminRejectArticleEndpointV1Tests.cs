namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle.V1;

/// <summary>
/// Integration tests for the AdminRejectArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RejectArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }
}
