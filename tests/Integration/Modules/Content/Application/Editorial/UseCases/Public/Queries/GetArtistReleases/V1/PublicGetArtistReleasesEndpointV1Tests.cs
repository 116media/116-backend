using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases.V1;

/// <summary>
/// Integration tests for the PublicGetArtistReleases endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArtistReleasesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetArtistReleases_WithUnknownSlug_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Releases("no-such-artist"));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetArtistReleases_SplitsAlbumsFromMixtapesByTypeFilter()
    {
        string slug = $"werrason-{Guid.NewGuid():N}";

        (AlbumEntity album, AlbumEntity mixtape) = await SeedAsync<ContentDbContext, (AlbumEntity, AlbumEntity)>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            AlbumEntity album = AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Album);
            AlbumEntity mixtape = AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Mixtape);

            ctx.Artists.Add(artist);
            ctx.Albums.AddRange(album, mixtape);

            return (album, mixtape);
        });

        Client.ClearAuthentication();

        var albumsResponse = await Client.GetAsync(Routes.Public.Artists.Releases(slug, "?type=Album"));
        PublicGetArtistReleasesResponse albums = await albumsResponse.ReadAsAsync<PublicGetArtistReleasesResponse>();
        albums.Releases.Items.Should().ContainSingle(a => a.Id == album.Id);
        albums.Releases.Items.Should().NotContain(a => a.Id == mixtape.Id);

        var mixtapesResponse = await Client.GetAsync(Routes.Public.Artists.Releases(slug, "?type=Mixtape"));
        PublicGetArtistReleasesResponse mixtapes =
            await mixtapesResponse.ReadAsAsync<PublicGetArtistReleasesResponse>();
        mixtapes.Releases.Items.Should().ContainSingle(a => a.Id == mixtape.Id);
        mixtapes.Releases.Items.Should().NotContain(a => a.Id == album.Id);
    }

    [Fact]
    public async Task GetArtistReleases_OrdersByYearDescendingWithUnknownYearsLast()
    {
        string slug = $"papa-wemba-{Guid.NewGuid():N}";

        (AlbumEntity newest, AlbumEntity oldest, AlbumEntity undated) = await SeedAsync<
            ContentDbContext,
            (AlbumEntity, AlbumEntity, AlbumEntity)
        >(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            AlbumEntity newest = AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Album, "Newest", 2024);
            AlbumEntity oldest = AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Album, "Oldest", 1998);
            AlbumEntity undated = AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Album, "Undated", null);

            ctx.Artists.Add(artist);
            ctx.Albums.AddRange(newest, oldest, undated);

            return (newest, oldest, undated);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Releases(slug, "?type=Album"));

        PublicGetArtistReleasesResponse body = await response.ReadAsAsync<PublicGetArtistReleasesResponse>();
        List<Guid> ids = body.Releases.Items.Select(a => a.Id).ToList();
        ids.Should().Equal(newest.Id, oldest.Id, undated.Id);
    }

    [Fact]
    public async Task GetArtistReleases_ForArtistWithNoReleases_ReturnsEmptyPageNot404()
    {
        string slug = $"no-releases-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Releases(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistReleasesResponse body = await response.ReadAsAsync<PublicGetArtistReleasesResponse>();
        body.Releases.Items.Should().BeEmpty();
        body.Releases.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetArtistReleases_SerialisesReleaseTypeByName()
    {
        string slug = $"enum-name-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, AlbumEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            AlbumEntity mixtape = AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Mixtape);
            ctx.Artists.Add(artist);
            ctx.Albums.Add(mixtape);
            return mixtape;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Releases(slug, "?type=Mixtape"));

        string raw = await response.Content.ReadAsStringAsync();
        raw.Should().Contain("\"Mixtape\"");
    }
}
