using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Exceptions;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.V1;

/// <summary>
/// Integration tests for the AdminResolveAlbumStreamingLinks endpoint, with the Odesli-backed
/// resolution service replaced by <see cref="StubStreamingLinkResolutionService" />, which
/// tests script through <see cref="StreamingStub" />.
/// </summary>
[Collection("Database")]
public class AdminResolveAlbumStreamingLinksEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string SourceUrl = "https://open.spotify.com/album/abc123";

    private StubStreamingLinkResolutionService StreamingStub =>
        Api.Services.GetRequiredService<StubStreamingLinkResolutionService>();

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
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(Guid.NewGuid()),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Album"))
        );
    }

    [Fact]
    public async Task ResolveAlbumStreamingLinks_PersistsOneCuratedRowPerResolvedPlatform()
    {
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

    [Fact]
    public async Task ResolveAlbumStreamingLinks_Twice_ReplacesWithoutDuplicating()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        await Client.PostAsJsonAsync(Url(album.Id), new AdminResolveAlbumStreamingLinksRequest(SourceUrl));

        StreamingStub.NextResult = new Dictionary<EnumStreamingPlatform, string>
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

    [Fact]
    public async Task ResolveAlbumStreamingLinks_LeavesManualRowsForUnresolvedPlatformsAlone()
    {
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

        StreamingStub.NextResult = new Dictionary<EnumStreamingPlatform, string>
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
        StreamingStub.NextResult = new Dictionary<EnumStreamingPlatform, string>();
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<StreamingLinkErrorMessage>(m => m.NothingResolved())
        );
    }

    [Fact]
    public async Task ResolveAlbumStreamingLinks_WhenProviderRateLimits_ReturnsTooManyRequests()
    {
        StreamingStub.NextException = new StreamingLinkResolutionException("slow down", isRateLimited: true);
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem<StreamingLinkResolutionException>(
            HttpStatusCode.TooManyRequests,
            Localized<StreamingLinkErrorMessage>(m => m.ResolutionRateLimited())
        );
    }

    [Fact]
    public async Task ResolveAlbumStreamingLinks_WhenProviderThrows_ReturnsBadGateway()
    {
        StreamingStub.NextException = new StreamingLinkResolutionException("provider down");
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem<StreamingLinkResolutionException>(
            HttpStatusCode.BadGateway,
            Localized<StreamingLinkErrorMessage>(m => m.ResolutionFailed())
        );
    }

    [Theory]
    [InlineData("http://open.spotify.com/album/abc")]
    [InlineData("not-a-url")]
    public async Task ResolveAlbumStreamingLinks_WithNonHttpsSourceUrl_ReturnsBadRequest(string sourceUrl)
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(album.Id),
            new AdminResolveAlbumStreamingLinksRequest(sourceUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
