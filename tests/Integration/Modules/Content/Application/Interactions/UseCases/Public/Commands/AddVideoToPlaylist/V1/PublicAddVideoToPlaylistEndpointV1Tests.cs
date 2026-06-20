namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist.V1;

/// <summary>
/// Integration tests for the PublicAddVideoToPlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicAddVideoToPlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AddVideoToPlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { VideoId = Guid.NewGuid(), SortOrder = 1 };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}/videos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddVideoToPlaylist_AsVisitor_NonExistentPlaylist_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var request = new { VideoId = Guid.NewGuid(), SortOrder = 1 };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}/videos", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
