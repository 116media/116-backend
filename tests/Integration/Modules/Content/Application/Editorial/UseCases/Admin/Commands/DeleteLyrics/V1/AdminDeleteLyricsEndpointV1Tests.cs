namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics.V1;

/// <summary>
/// Integration tests for the AdminDeleteLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeleteLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLyrics_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
