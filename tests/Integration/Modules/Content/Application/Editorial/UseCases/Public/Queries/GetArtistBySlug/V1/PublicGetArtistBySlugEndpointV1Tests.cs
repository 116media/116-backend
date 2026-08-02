using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

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

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Full round trip: an artist profile with a published lyrics page and a published video
    /// linked to it must both appear in the artist's public page response.
    /// </summary>
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

    /// <summary>
    /// An artist page must show only <c>Published</c> lyrics — a Draft lyrics page linked to
    /// the same artist must never appear in the paginated result.
    /// </summary>
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

    /// <summary>
    /// Symmetric to the lyrics case: a Draft video linked to the artist must never appear
    /// among the artist page's published videos.
    /// </summary>
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
}
