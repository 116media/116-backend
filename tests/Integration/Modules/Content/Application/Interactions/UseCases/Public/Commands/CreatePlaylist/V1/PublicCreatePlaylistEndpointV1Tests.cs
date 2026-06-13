namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist.V1;

/// <summary>
/// Integration tests for the PublicCreatePlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicCreatePlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreatePlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "My Test Playlist" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Public.Playlists, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePlaylist_AsVisitor_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Name = "" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Public.Playlists, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreatePlaylist_AsVisitor_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Name = "My Test Playlist" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Public.Playlists, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }
}
