using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetMyPlaylists.V1;

/// <summary>
/// Integration tests for the PublicGetMyPlaylists endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetMyPlaylistsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<PlaylistEntity> SeedPlaylistAsync(Guid userId)
    {
        return await SeedAsync<ContentDbContext, PlaylistEntity>(ctx =>
        {
            PlaylistEntity playlist = PlaylistFactory.Create(userId);
            ctx.Playlists.Add(playlist);
            return playlist;
        });
    }

    [Fact]
    public async Task GetMyPlaylists_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyPlaylists_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<PlaylistDto> body = await response.ReadAsAsync<List<PlaylistDto>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMyPlaylists_AsVisitor_ReturnsOwnedPlaylist()
    {
        PlaylistEntity playlist = await SeedPlaylistAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<PlaylistDto> body = await response.ReadAsAsync<List<PlaylistDto>>();
        body.Should().ContainSingle(p => p.Id == playlist.Id && p.Name == playlist.Name);
    }
}
