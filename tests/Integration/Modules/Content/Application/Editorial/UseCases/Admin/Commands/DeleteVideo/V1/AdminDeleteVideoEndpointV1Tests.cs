namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo.V1;

/// <summary>
/// Integration tests for the AdminDeleteVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeleteVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
