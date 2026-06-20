namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo.V1;

/// <summary>
/// Integration tests for the AdminPublishVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminPublishVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublishVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublishVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
