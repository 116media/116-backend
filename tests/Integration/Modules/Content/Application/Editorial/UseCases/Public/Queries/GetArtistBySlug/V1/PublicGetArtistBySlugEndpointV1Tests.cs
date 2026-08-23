using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetArtistBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArtistBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetArtistBySlug_WithNonExistentSlug_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.BySlug("non-existent-artist"));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Artist"))
        );
    }

    [Fact]
    public async Task GetArtistBySlug_WithPublishedCatalog_ReturnsArtistLyricsAndVideos()
    {
        string slug = $"fally-ipupa-{Guid.NewGuid():N}";

        (ArtistEntity artist, LyricsEntity lyrics, VideoEntity video) = await SeedAsync<
            ContentDbContext,
            (ArtistEntity, LyricsEntity, VideoEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            LyricsEntity lyrics = LyricsFactory.CreatePublishedForArtist(category.Id, artist.Id);
            VideoEntity video = VideoFactory.CreatePublishedForArtist(category.Id, artist.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(lyrics);
            ctx.Videos.Add(video);

            return (artist, lyrics, video);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.BySlug(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArtistBySlugResponse body = await response.ReadAsAsync<PublicGetArtistBySlugResponse>();
        body.Artist.Id.Should().Be(artist.Id);
        body.Artist.Slug.Should().Be(slug);
        body.Lyrics.Items.Should().ContainSingle(l => l.Id == lyrics.Id);
        body.Videos.Items.Should().ContainSingle(v => v.Id == video.Id);
    }

    [Fact]
    public async Task GetArtistBySlug_WithDraftLyricsLinkedToArtist_ExcludesDraftFromResult()
    {
        string slug = $"koffi-olomide-{Guid.NewGuid():N}";

        (ArtistEntity artist, LyricsEntity publishedLyrics) = await SeedAsync<
            ContentDbContext,
            (ArtistEntity, LyricsEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            LyricsEntity publishedLyrics = LyricsFactory.CreatePublishedForArtist(category.Id, artist.Id);
            LyricsEntity draftLyrics = LyricsFactory.CreateForArtist(category.Id, artist.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Lyrics.AddRange(publishedLyrics, draftLyrics);

            return (artist, publishedLyrics);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.BySlug(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArtistBySlugResponse body = await response.ReadAsAsync<PublicGetArtistBySlugResponse>();
        body.Lyrics.Items.Should().ContainSingle();
        body.Lyrics.Items.Should().OnlyContain(l => l.Id == publishedLyrics.Id);
    }

    [Fact]
    public async Task GetArtistBySlug_WithDraftVideoLinkedToArtist_ExcludesDraftFromResult()
    {
        string slug = $"werrason-{Guid.NewGuid():N}";

        (ArtistEntity artist, VideoEntity publishedVideo) = await SeedAsync<
            ContentDbContext,
            (ArtistEntity, VideoEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            VideoEntity publishedVideo = VideoFactory.CreatePublishedForArtist(category.Id, artist.Id);
            VideoEntity draftVideo = VideoFactory.CreateForArtist(category.Id, artist.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Videos.AddRange(publishedVideo, draftVideo);

            return (artist, publishedVideo);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.BySlug(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArtistBySlugResponse body = await response.ReadAsAsync<PublicGetArtistBySlugResponse>();
        body.Videos.Items.Should().ContainSingle();
        body.Videos.Items.Should().OnlyContain(v => v.Id == publishedVideo.Id);
    }

    #region Totals, 404 Rule and Verification

    [Fact]
    public async Task GetArtistBySlug_WithZeroContentEverywhere_ReturnsNotFound()
    {
        string slug = $"stub-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.Create($"Stub {Guid.NewGuid():N}", slug, "A nice biography.");
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.BySlug(slug));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Artist"))
        );
    }

    [Fact]
    public async Task GetArtistBySlug_WithOnlyAnAlbum_Returns200WithAlbumTotal()
    {
        string slug = $"album-only-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            AlbumEntity album = AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Album);
            ctx.Artists.Add(artist);
            ctx.Albums.Add(album);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.BySlug(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistBySlugResponse body = await response.ReadAsAsync<PublicGetArtistBySlugResponse>();
        body.Totals.Albums.Should().Be(1);
        body.Totals.Songs.Should().Be(0);
        body.Totals.Videos.Should().Be(0);
        body.Totals.Mixtapes.Should().Be(0);
        body.Totals.News.Should().Be(0);
    }

    [Fact]
    public async Task GetArtistBySlug_ReturnsIdentitySocialLinksAndVerification()
    {
        string slug = $"identity-{Guid.NewGuid():N}";
        var birthdate = new DateOnly(1986, 10, 24);

        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.CreateWithIdentity(
                "Aubrey Drake Graham",
                ["Drizzy", "Champagne Papi"],
                birthdate,
                "Toronto, Canada"
            );

            // Rebuild with the known slug and a claim, keeping the identity fields.
            artist = ArtistEntity.Create(
                artist.Id,
                artist.Name,
                slug,
                artist.Bio,
                artist.RealName,
                artist.Aliases,
                artist.Birthdate,
                artist.Hometown,
                TestErrorsFactory.CreateArtistErrors()
            );
            artist.ClaimOwnership(Guid.NewGuid(), TestErrorsFactory.CreateArtistErrors());

            LyricsEntity lyrics = LyricsFactory.CreatePublishedForArtist(category.Id, artist.Id);
            ArtistSocialLinkEntity link = ArtistSocialLinkEntity.Create(
                Guid.NewGuid(),
                artist.Id,
                EnumSocialPlatform.Instagram,
                "https://instagram.com/champagnepapi"
            );

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(lyrics);
            ctx.ArtistSocialLinks.Add(link);

            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.BySlug(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistBySlugResponse body = await response.ReadAsAsync<PublicGetArtistBySlugResponse>();

        body.Artist.RealName.Should().Be("Aubrey Drake Graham");
        body.Artist.Aliases.Should().Equal("Drizzy", "Champagne Papi");
        body.Artist.Birthdate.Should().Be(birthdate);
        body.Artist.Hometown.Should().Be("Toronto, Canada");
        body.Artist.IsVerified.Should().BeTrue();
        body.Artist.SocialLinks.Should().ContainSingle(l => l.Url == "https://instagram.com/champagnepapi");

        string raw = await response.Content.ReadAsStringAsync();
        raw.Should().NotContainEquivalentOf("\"userId\"");
    }

    #endregion
}
