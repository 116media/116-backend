namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist.V1;

/// <summary>
/// Integration tests for the PublicRemoveVideoFromPlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicRemoveVideoFromPlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RemoveVideoFromPlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}/videos/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveVideoFromPlaylist_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}/videos/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
