using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.DeletePlaylist.V1;

/// <summary>
/// Integration tests for the PublicDeletePlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicDeletePlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeletePlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePlaylist_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that a visitor attempting to delete a playlist owned by a different user
    /// receives a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task DeletePlaylist_AsVisitor_WhenNotOwner_ReturnsBadRequest()
    {
        var otherUserId = User.AdminId;

        await using var seedContext = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(otherUserId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Playlists}/{playlist.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
