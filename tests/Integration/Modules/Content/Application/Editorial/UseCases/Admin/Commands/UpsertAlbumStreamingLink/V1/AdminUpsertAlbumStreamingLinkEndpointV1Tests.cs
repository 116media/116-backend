using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink.V1;

/// <summary>
/// Integration tests for the AdminUpsertAlbumStreamingLink endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpsertAlbumStreamingLinkEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string SpotifyUrl = "https://open.spotify.com/album/first-curated";
    private const string ReplacementUrl = "https://open.spotify.com/album/second-curated";

    /// <summary>
    /// Builds the album streaming link route for the given album and platform.
    /// </summary>
    /// <param name="albumId">The album the link belongs to.</param>
    /// <param name="platform">The streaming platform slot.</param>
    /// <returns>The fully qualified endpoint URL.</returns>
    private static string Url(Guid albumId, EnumStreamingPlatform platform) =>
        Routes.Admin.Editorial.StreamingLink(EditorialRouteConstants.Albums, albumId, platform.ToString());

    /// <summary>
    /// Seeds a standalone album.
    /// </summary>
    /// <returns>The seeded album.</returns>
    private async Task<AlbumEntity> SeedAlbumAsync()
    {
        return await SeedAsync<ContentDbContext, AlbumEntity>(ctx =>
        {
            AlbumEntity album = AlbumFactory.Create();
            ctx.Albums.Add(album);
            return album;
        });
    }

    [Fact]
    public async Task UpsertAlbumStreamingLink_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify),
            new AdminUpsertAlbumStreamingLinkRequest(SpotifyUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpsertAlbumStreamingLink_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify),
            new AdminUpsertAlbumStreamingLinkRequest(SpotifyUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpsertAlbumStreamingLink_AsAdmin_WithNonExistentAlbum_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify),
            new AdminUpsertAlbumStreamingLinkRequest(SpotifyUrl)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// The first upsert for an empty platform slot inserts a new curated link row bound to the
    /// album, leaving the single-side association unset.
    /// </summary>
    [Fact]
    public async Task UpsertAlbumStreamingLink_WhenSlotEmpty_CreatesLinkAndPersists()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(album.Id, EnumStreamingPlatform.Spotify),
            new AdminUpsertAlbumStreamingLinkRequest(SpotifyUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpsertAlbumStreamingLinkResponse body =
            await response.ReadAsAsync<AdminUpsertAlbumStreamingLinkResponse>();
        body.StreamingLinkId.Should().NotBeEmpty();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        StreamingLinkEntity? persisted = await ctx.StreamingLinks.FirstOrDefaultAsync(link =>
            link.Id == body.StreamingLinkId
        );

        persisted.Should().NotBeNull();
        persisted!.AlbumId.Should().Be(album.Id);
        persisted.LyricsId.Should().BeNull();
        persisted.Platform.Should().Be(EnumStreamingPlatform.Spotify);
        persisted.Url.Should().Be(SpotifyUrl);
    }

    /// <summary>
    /// A second upsert on an already-populated platform slot replaces the curated URL on the
    /// existing row instead of inserting a duplicate.
    /// </summary>
    [Fact]
    public async Task UpsertAlbumStreamingLink_WhenSlotAlreadySet_ReplacesUrlWithoutDuplicating()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var firstResponse = await Client.PutAsJsonAsync(
            Url(album.Id, EnumStreamingPlatform.Spotify),
            new AdminUpsertAlbumStreamingLinkRequest(SpotifyUrl)
        );
        AdminUpsertAlbumStreamingLinkResponse firstBody =
            await firstResponse.ReadAsAsync<AdminUpsertAlbumStreamingLinkResponse>();

        var response = await Client.PutAsJsonAsync(
            Url(album.Id, EnumStreamingPlatform.Spotify),
            new AdminUpsertAlbumStreamingLinkRequest(ReplacementUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpsertAlbumStreamingLinkResponse body =
            await response.ReadAsAsync<AdminUpsertAlbumStreamingLinkResponse>();
        body.StreamingLinkId.Should().Be(firstBody.StreamingLinkId);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<StreamingLinkEntity> persisted = await ctx
            .StreamingLinks.Where(link => link.AlbumId == album.Id)
            .ToListAsync();

        persisted.Should().ContainSingle();
        persisted[0].Url.Should().Be(ReplacementUrl);
    }

    /// <summary>
    /// Each platform owns an independent slot on the same album, so setting a second platform
    /// does not disturb the first.
    /// </summary>
    [Fact]
    public async Task UpsertAlbumStreamingLink_ForSecondPlatform_CreatesSeparateRow()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Url(album.Id, EnumStreamingPlatform.Spotify),
            new AdminUpsertAlbumStreamingLinkRequest(SpotifyUrl)
        );

        var response = await Client.PutAsJsonAsync(
            Url(album.Id, EnumStreamingPlatform.Tidal),
            new AdminUpsertAlbumStreamingLinkRequest("https://tidal.com/browse/album/curated")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<StreamingLinkEntity> persisted = await ctx
            .StreamingLinks.Where(link => link.AlbumId == album.Id)
            .ToListAsync();

        persisted.Should().HaveCount(2);
        persisted.Should().Contain(link => link.Platform == EnumStreamingPlatform.Spotify);
        persisted.Should().Contain(link => link.Platform == EnumStreamingPlatform.Tidal);
    }
}
