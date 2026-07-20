using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnPlaylists.V1;

/// <summary>
/// Integration tests for the PublicGetOwnPlaylists endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnPlaylistsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetOwnPlaylists_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnPlaylists_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<PlaylistDto> body = await response.ReadAsAsync<List<PlaylistDto>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOwnPlaylists_AsVisitor_ReturnsOwnedPlaylist()
    {
        PlaylistEntity playlist = await SeedPlaylistAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<PlaylistDto> body = await response.ReadAsAsync<List<PlaylistDto>>();
        body.Should().ContainSingle(p => p.Id == playlist.Id && p.Name == playlist.Name);
    }

    [Fact]
    public async Task GetOwnPlaylists_ReturnsFirstFourNullableThumbnailSlotsInPlaylistOrder()
    {
        PlaylistEntity playlist;
        VideoEntity[] videos;
        FileEntity firstThumbnail = FileFactory.Create();
        FileEntity thirdThumbnail = FileFactory.Create();
        FileEntity fifthThumbnail = FileFactory.Create();
        await using (ContentDbContext context = CreateDbContext<ContentDbContext>())
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            context.ContentTypes.Add(contentType);
            await context.SaveChangesAsync();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            videos = VideoFactory.CreateManyPublished(category.Id, 5).ToArray();
            videos[0].SetThumbnailFileId(firstThumbnail.Id);
            videos[2].SetThumbnailFileId(thirdThumbnail.Id);
            videos[4].SetThumbnailFileId(fifthThumbnail.Id);
            context.Videos.AddRange(videos);
            playlist = PlaylistFactory.Create(TestUser.VisitorId);
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();
            context.PlaylistVideos.AddRange(
                videos.Select(
                    (video, index) => PlaylistVideoEntity.Create(Guid.NewGuid(), playlist.Id, video.Id, index)
                )
            );
            await context.SaveChangesAsync();
        }
        await using (CoreDbContext context = CreateDbContext<CoreDbContext>())
        {
            context.Files.AddRange(firstThumbnail, thirdThumbnail, fifthThumbnail);
            await context.SaveChangesAsync();
        }
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<PlaylistDto> body = await response.ReadAsAsync<List<PlaylistDto>>();
        PlaylistDto item = body.Should().ContainSingle(dto => dto.Id == playlist.Id).Subject;
        item.VideoCount.Should().Be(5);
        item.ThumbnailUrls.Should()
            .Equal(new string?[] { firstThumbnail.StorageUrl, null, thirdThumbnail.StorageUrl, null });
    }
}
