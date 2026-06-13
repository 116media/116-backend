namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist.V1;

/// <summary>
/// Integration tests for the PublicRenamePlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicRenamePlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RenamePlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Updated Playlist" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RenamePlaylist_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Name = "Updated Playlist" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
