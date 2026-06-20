namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById.V1;

/// <summary>
/// Integration tests for the AdminGetVideoById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetVideoByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetVideoById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVideoById_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetVideoById_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
