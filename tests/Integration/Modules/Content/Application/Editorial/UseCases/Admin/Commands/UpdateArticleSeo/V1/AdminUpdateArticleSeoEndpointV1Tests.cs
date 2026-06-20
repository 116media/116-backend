namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo.V1;

/// <summary>
/// Integration tests for the AdminUpdateArticleSeo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateArticleSeoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateArticleSeo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/seo",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateArticleSeo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/seo",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateArticleSeo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/seo",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
