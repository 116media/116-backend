namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;

/// <summary>
/// Integration tests for the AdminUpdateVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", new { });

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }
}
