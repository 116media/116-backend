using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetPlaylistById.V1;

/// <summary>
/// Integration tests for the PublicGetPlaylistById endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPlaylistByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetPlaylistById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Playlists.ById(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPlaylistById_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(Routes.Public.Playlists.ById(Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPlaylistById_AsVisitor_NotOwner_ReturnsBadRequest()
    {
        VideoEntity video = await SeedPublishedVideoAsync();
        PlaylistEntity playlist = await SeedPlaylistWithVideoAsync(TestUser.AdminId, video.Id);
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(Routes.Public.Playlists.ById(playlist.Id));

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPlaylistById_AsOwner_ReturnsPlaylistWithVideos()
    {
        VideoEntity video = await SeedPublishedVideoAsync();
        PlaylistEntity playlist = await SeedPlaylistWithVideoAsync(TestUser.VisitorId, video.Id);
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(Routes.Public.Playlists.ById(playlist.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PlaylistDetailDto body = await response.ReadAsAsync<PlaylistDetailDto>();
        body.Id.Should().Be(playlist.Id);
        body.Name.Should().Be(playlist.Name);
        VideoInPlaylistDto item = body.Videos.Should().ContainSingle(v => v.VideoId == video.Id).Subject;
        item.Slug.Should().Be(video.Slug);
        item.CategoryName.Should().NotBeNullOrWhiteSpace();
        item.PublishedAt.Should().NotBeNull();
        video.PublishedAt.Should().NotBeNull();
        item.PublishedAt.GetValueOrDefault()
            .Should()
            .BeCloseTo(video.PublishedAt.GetValueOrDefault(), TimeSpan.FromMicroseconds(1));
    }
}
