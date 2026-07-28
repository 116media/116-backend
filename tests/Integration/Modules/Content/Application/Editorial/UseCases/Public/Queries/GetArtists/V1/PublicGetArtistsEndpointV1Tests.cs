using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtists.V1;

/// <summary>
/// Integration tests for the PublicGetArtists directory endpoint. Every test seeds through
/// the real DbContext and reads through real HTTP, so the content predicate, the folded
/// ordering and the per-card count are all exercised as one SQL statement.
/// </summary>
[Collection("Database")]
public class PublicGetArtistsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Seeds an artist with one published lyrics page so it clears the content predicate.
    /// </summary>
    private async Task<ArtistEntity> SeedListedArtistAsync(string name)
    {
        return await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.Create(name, $"slug-{Guid.NewGuid():N}");
            LyricsEntity lyrics = LyricsFactory.CreatePublishedForArtist(category.Id, artist.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(lyrics);

            return artist;
        });
    }

    [Fact]
    public async Task GetArtists_WithNoContent_ExcludesTheArtistEntirely()
    {
        // An artist row with a bio but nothing published is a stub, not a destination.
        ArtistEntity stub = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.Create($"Stub {Guid.NewGuid():N}", $"stub-{Guid.NewGuid():N}");
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?pageSize=100"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistsResponse body = await response.ReadAsAsync<PublicGetArtistsResponse>();
        body.Artists.Items.Should().NotContain(a => a.Slug == stub.Slug);
    }

    [Fact]
    public async Task GetArtists_WithPublishedContent_ListsTheArtistWithItsCount()
    {
        ArtistEntity artist = await SeedListedArtistAsync($"Listed {Guid.NewGuid():N}");

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?pageSize=100"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistsResponse body = await response.ReadAsAsync<PublicGetArtistsResponse>();
        var card = body.Artists.Items.SingleOrDefault(a => a.Slug == artist.Slug);
        card.Should().NotBeNull();
        card!.ContentCount.Should().Be(1);
        card.Name.Should().Be(artist.Name);
    }

    /// <summary>
    /// The response never exposes an artist id — the whole public surface is slug-addressed.
    /// </summary>
    [Fact]
    public async Task GetArtists_ResponseBody_CarriesNoArtistIds()
    {
        await SeedListedArtistAsync($"NoIds {Guid.NewGuid():N}");

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?pageSize=100"));

        string raw = await response.Content.ReadAsStringAsync();
        raw.Should().NotContainEquivalentOf("\"id\":");
        raw.Should().NotContainEquivalentOf("\"userId\":");
    }

    [Fact]
    public async Task GetArtists_WithAccentedName_SortsBucketsAndSearchesAccentInsensitively()
    {
        string unique = Guid.NewGuid().ToString("N")[..8];
        ArtistEntity accented = await SeedListedArtistAsync($"Élodie {unique}");

        Client.ClearAuthentication();

        // Buckets under E, not under a separate accented bucket.
        var letterResponse = await Client.GetAsync(Routes.Public.Artists.Directory("?letter=E&pageSize=100"));
        PublicGetArtistsResponse letterBody = await letterResponse.ReadAsAsync<PublicGetArtistsResponse>();
        letterBody.Artists.Items.Should().Contain(a => a.Slug == accented.Slug);
        letterBody.AvailableLetters.Should().Contain("E");

        // Matches an unaccented search.
        var searchResponse = await Client.GetAsync(
            Routes.Public.Artists.Directory($"?search=elodie {unique}&pageSize=100")
        );
        PublicGetArtistsResponse searchBody = await searchResponse.ReadAsAsync<PublicGetArtistsResponse>();
        searchBody.Artists.Items.Should().Contain(a => a.Slug == accented.Slug);
    }

    [Fact]
    public async Task GetArtists_SearchMatchesNameButNeverBio()
    {
        string marker = Guid.NewGuid().ToString("N")[..10];

        // An artist whose BIO carries the marker but whose name does not.
        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.Create(
                $"BioOnly {Guid.NewGuid():N}",
                $"slug-{Guid.NewGuid():N}",
                $"Biography mentioning bio-{marker}"
            );
            LyricsEntity lyrics = LyricsFactory.CreatePublishedForArtist(category.Id, artist.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(lyrics);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory($"?search={marker}&pageSize=100"));

        PublicGetArtistsResponse body = await response.ReadAsAsync<PublicGetArtistsResponse>();
        body.Artists.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetArtists_WithLetterAndSearchTogether_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?letter=F&search=fally"));

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
