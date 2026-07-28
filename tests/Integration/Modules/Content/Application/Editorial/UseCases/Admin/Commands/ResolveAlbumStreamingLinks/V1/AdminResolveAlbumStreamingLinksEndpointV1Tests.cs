using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.V1;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.V1;

/// <summary>
/// Integration tests for the AdminResolveAlbumStreamingLinks endpoint, with the Odesli-backed
/// resolution service replaced by <see cref="StubStreamingLinkResolutionService"/> — the
/// external-service stub exception, exactly as Cloudinary is stubbed.
/// </summary>
[Collection("Database")]
public class AdminResolveAlbumStreamingLinksEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string SourceUrl = "https://open.spotify.com/album/abc123";

    private static string Url(Guid albumId) =>
        Routes.Admin.Editorial.ResolveStreamingLinks(EditorialRouteConstants.Albums, albumId);

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
    public async Task ResolveAlbumStreamingLinks_WithNoAuth_ReturnsUnauthorized()
    {
        StubStreamingLinkResolutionService.Reset();
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Url(Guid.NewGuid()),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResolveAlbumStreamingLinks_WithUnknownAlbum_ReturnsNotFound()
    {
        StubStreamingLinkResolutionService.Reset();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(Guid.NewGuid()),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// The happy path: one paste persists a curated row per resolved platform, readable back
    /// from the database.
    /// </summary>
    [Fact]
    public async Task ResolveAlbumStreamingLinks_PersistsOneCuratedRowPerResolvedPlatform()
    {
        StubStreamingLinkResolutionService.Reset();
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminResolveAlbumStreamingLinksResponse body =
            await response.ReadAsAsync<AdminResolveAlbumStreamingLinksResponse>();
        body.Resolved.Should().BeEquivalentTo(Enum.GetValues<EnumStreamingPlatform>());
        body.Unresolved.Should().BeEmpty();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<StreamingLinkEntity> rows = await ctx.StreamingLinks.Where(l => l.AlbumId == album.Id).ToListAsync();
        rows.Should().HaveCount(Enum.GetValues<EnumStreamingPlatform>().Length);
        rows.Should().OnlyContain(l => l.Url.StartsWith("https://resolved.example/"));
    }

    /// <summary>
    /// A second resolve replaces URLs on the existing rows — never a duplicate per platform.
    /// </summary>
    [Fact]
    public async Task ResolveAlbumStreamingLinks_Twice_ReplacesWithoutDuplicating()
    {
        StubStreamingLinkResolutionService.Reset();
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        await Client.PostAsJsonAsync(Url(album.Id), new AdminResolveAlbumStreamingLinksRequest(SourceUrl));

        StubStreamingLinkResolutionService.NextResult = new Dictionary<EnumStreamingPlatform, string>
        {
            [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/album/replaced",
        };
        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<StreamingLinkEntity> spotifyRows = await ctx
            .StreamingLinks.Where(l => l.AlbumId == album.Id && l.Platform == EnumStreamingPlatform.Spotify)
            .ToListAsync();
        spotifyRows.Should().HaveCount(1);
        spotifyRows[0].Url.Should().Be("https://open.spotify.com/album/replaced");
    }

    /// <summary>
    /// Resolution never deletes: a hand-curated row for a platform the provider had no link
    /// for survives the resolve untouched.
    /// </summary>
    [Fact]
    public async Task ResolveAlbumStreamingLinks_LeavesManualRowsForUnresolvedPlatformsAlone()
    {
        StubStreamingLinkResolutionService.Reset();
        AlbumEntity album = await SeedAlbumAsync();
        StreamingLinkEntity manualTidal = await SeedAsync<ContentDbContext, StreamingLinkEntity>(ctx =>
        {
            StreamingLinkEntity link = StreamingLinkEntity.ForAlbum(
                Guid.NewGuid(),
                album.Id,
                EnumStreamingPlatform.Tidal,
                "https://listen.tidal.com/album/hand-picked"
            );
            ctx.StreamingLinks.Add(link);
            return link;
        });

        StubStreamingLinkResolutionService.NextResult = new Dictionary<EnumStreamingPlatform, string>
        {
            [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/album/1",
        };
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AdminResolveAlbumStreamingLinksResponse body =
            await response.ReadAsAsync<AdminResolveAlbumStreamingLinksResponse>();
        body.Unresolved.Should().Contain(EnumStreamingPlatform.Tidal);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        StreamingLinkEntity? tidal = await ctx.StreamingLinks.SingleOrDefaultAsync(l => l.Id == manualTidal.Id);
        tidal.Should().NotBeNull();
        tidal!.Url.Should().Be("https://listen.tidal.com/album/hand-picked");
    }

    [Fact]
    public async Task ResolveAlbumStreamingLinks_WhenProviderResolvesNothing_ReturnsNotFound()
    {
        StubStreamingLinkResolutionService.Reset();
        StubStreamingLinkResolutionService.NextResult = new Dictionary<EnumStreamingPlatform, string>();
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResolveAlbumStreamingLinks_WhenProviderThrows_ReturnsBadGateway()
    {
        StubStreamingLinkResolutionService.Reset();
        StubStreamingLinkResolutionService.NextException = new StreamingLinkResolutionException("provider down");
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem(HttpStatusCode.BadGateway);
    }

    [Theory]
    [InlineData("http://open.spotify.com/album/abc")]
    [InlineData("not-a-url")]
    public async Task ResolveAlbumStreamingLinks_WithNonHttpsSourceUrl_ReturnsBadRequest(string sourceUrl)
    {
        StubStreamingLinkResolutionService.Reset();
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(sourceUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
