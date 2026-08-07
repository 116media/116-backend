using _116.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist.V1;

/// <summary>
/// Integration tests for the PublicAddVideoToPlaylist endpoint.
/// </summary>
[Collection("Database")]
public class PublicAddVideoToPlaylistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task AddVideoToPlaylist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        PublicAddVideoToPlaylistRequest request = new PublicAddVideoToPlaylistRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Playlists.Videos(Guid.NewGuid()), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddVideoToPlaylist_AsVisitor_NonExistentPlaylist_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        PublicAddVideoToPlaylistRequest request = new PublicAddVideoToPlaylistRequestBuilder().Build();
        var missingId = Guid.NewGuid();

        var response = await Client.PostAsJsonAsync(Routes.Public.Playlists.Videos(missingId), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<PlaylistErrorMessage>(m => m.NotFound(missingId))
        );
    }

    [Fact]
    public async Task AddVideoToPlaylist_AsVisitor_WithValidData_AddsJoinRow()
    {
        VideoEntity video = await SeedPublishedVideoAsync();
        PlaylistEntity playlist = await SeedPlaylistAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();
        PublicAddVideoToPlaylistRequest request = new PublicAddVideoToPlaylistRequestBuilder()
            .WithVideoId(video.Id)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Playlists.Videos(playlist.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicAddVideoToPlaylistResponse body = await response.ReadAsAsync<PublicAddVideoToPlaylistResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.PlaylistVideos.AnyAsync(pv => pv.PlaylistId == playlist.Id && pv.VideoId == video.Id))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task AddVideo_WhenAlreadyInPlaylist_ReturnsConflict()
    {
        VideoEntity video = await SeedPublishedVideoAsync();
        PlaylistEntity playlist = await SeedPlaylistAsync(TestUser.VisitorId);

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.PlaylistVideos.Add(PlaylistVideoEntity.Create(Guid.NewGuid(), playlist.Id, video.Id, sortOrder: 1));
        });

        Client.AuthenticateAsVisitor();
        PublicAddVideoToPlaylistRequest request = new PublicAddVideoToPlaylistRequestBuilder()
            .WithVideoId(video.Id)
            .WithSortOrder(2)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Playlists.Videos(playlist.Id), request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<PlaylistErrorMessage>(m => m.VideoAlreadyInPlaylist())
        );

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.PlaylistVideos.CountAsync(pv => pv.PlaylistId == playlist.Id && pv.VideoId == video.Id))
            .Should()
            .Be(1);
    }
}
