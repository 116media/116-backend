using _116.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist.V1;

/// <summary>
/// Integration tests for the PublicRemoveVideoFromPlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicRemoveVideoFromPlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<VideoEntity> SeedPublishedVideoAsync()
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            VideoEntity video = VideoFactory.CreatePublished(category.Id);
            ctx.Videos.Add(video);
            return video;
        });
    }

    private async Task<PlaylistEntity> SeedPlaylistWithVideoAsync(Guid userId, Guid videoId)
    {
        return await SeedAsync<ContentDbContext, PlaylistEntity>(ctx =>
        {
            PlaylistEntity playlist = PlaylistFactory.Create(userId);
            ctx.Playlists.Add(playlist);
            ctx.PlaylistVideos.Add(PlaylistVideoEntity.Create(Guid.NewGuid(), playlist.Id, videoId, sortOrder: 1));
            return playlist;
        });
    }

    [Fact]
    public async Task RemoveVideoFromPlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Public.Playlists.Video(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveVideoFromPlaylist_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var missingPlaylistId = Guid.NewGuid();

        var response = await Client.DeleteAsync(Routes.Public.Playlists.Video(missingPlaylistId, Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<PlaylistErrorMessage>(m => m.NotFound(missingPlaylistId))
        );
    }

    [Fact]
    public async Task RemoveVideoFromPlaylist_AsVisitor_NotOwner_ReturnsBadRequest()
    {
        VideoEntity video = await SeedPublishedVideoAsync();
        PlaylistEntity playlist = await SeedPlaylistWithVideoAsync(TestUser.AdminId, video.Id);
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Playlists.Video(playlist.Id, video.Id));

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<PlaylistErrorMessage>(m => m.NotOwner())
        );
    }

    [Fact]
    public async Task RemoveVideoFromPlaylist_AsOwner_RemovesJoinRow()
    {
        VideoEntity video = await SeedPublishedVideoAsync();
        PlaylistEntity playlist = await SeedPlaylistWithVideoAsync(TestUser.VisitorId, video.Id);
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Playlists.Video(playlist.Id, video.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicRemoveVideoFromPlaylistResponse body =
            await response.ReadAsAsync<PublicRemoveVideoFromPlaylistResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.PlaylistVideos.AnyAsync(pv => pv.PlaylistId == playlist.Id && pv.VideoId == video.Id))
            .Should()
            .BeFalse();
    }
}
