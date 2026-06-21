using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

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

    /// <summary>
    /// Verifies that adding a video that is already in the playlist returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task AddVideo_WhenAlreadyInPlaylist_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var video = VideoFactory.CreatePublished(category.Id);
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var playlist = PlaylistFactory.Create(User.VisitorId);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        var playlistVideo = PlaylistVideoEntity.Create(Guid.NewGuid(), playlist.Id, video.Id, sortOrder: 1);
        context.PlaylistVideos.Add(playlistVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();
        var request = new { VideoId = video.Id, SortOrder = 2 };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Playlists}/{playlist.Id}/videos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
