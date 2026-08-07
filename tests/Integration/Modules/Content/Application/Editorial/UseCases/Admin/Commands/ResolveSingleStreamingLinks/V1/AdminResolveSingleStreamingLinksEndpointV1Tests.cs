using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.V1;

/// <summary>
/// Integration tests for the AdminResolveSingleStreamingLinks endpoint, with the resolution
/// service stubbed.
/// </summary>
[Collection("Database")]
public class AdminResolveSingleStreamingLinksEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string SourceUrl = "https://open.spotify.com/track/xyz789";

    private StubStreamingLinkResolutionService StreamingStub =>
        Api.Services.GetRequiredService<StubStreamingLinkResolutionService>();

    private static string Url(Guid lyricsId) =>
        Routes.Admin.Editorial.ResolveStreamingLinks(EditorialRouteConstants.Lyrics, lyricsId);

    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            return category.Id;
        });
    }

    [Fact]
    public async Task ResolveSingleStreamingLinks_ForStandaloneSingle_PersistsCuratedRows()
    {
        StreamingStub.NextResult = new Dictionary<EnumStreamingPlatform, string>
        {
            [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/track/1",
            [EnumStreamingPlatform.Deezer] = "https://www.deezer.com/track/5",
        };

        Guid categoryId = await SeedCategoryAsync();
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(lyrics.Id),
            new AdminResolveSingleStreamingLinksRequest(SourceUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminResolveSingleStreamingLinksResponse body =
            await response.ReadAsAsync<AdminResolveSingleStreamingLinksResponse>();
        body.Resolved.Should().Equal(EnumStreamingPlatform.Spotify, EnumStreamingPlatform.Deezer);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<StreamingLinkEntity> rows = await ctx.StreamingLinks.Where(l => l.LyricsId == lyrics.Id).ToListAsync();
        rows.Should().HaveCount(2);
        rows.Should().OnlyContain(l => l.AlbumId == null);
    }

    [Fact]
    public async Task ResolveSingleStreamingLinks_WhenSongBelongsToAlbum_ReturnsConflict()
    {
        Guid categoryId = await SeedCategoryAsync();
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            AlbumEntity album = AlbumFactory.Create();
            LyricsEntity entity = LyricsFactory.CreateForAlbum(categoryId, album.Id);
            ctx.Albums.Add(album);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(lyrics.Id),
            new AdminResolveSingleStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<LyricsErrorMessage>(m => m.BelongsToAlbum())
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        (await ctx.StreamingLinks.AnyAsync(l => l.LyricsId == lyrics.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task ResolveSingleStreamingLinks_WithUnknownSong_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(Guid.NewGuid()),
            new AdminResolveSingleStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task ResolveSingleStreamingLinks_ResolvedLinksSurfaceOnThePublicLyricsPage()
    {
        StreamingStub.NextResult = new Dictionary<EnumStreamingPlatform, string>
        {
            [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/track/curated-abc",
        };

        Guid categoryId = await SeedCategoryAsync();
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();
        await Client.PostAsJsonAsync(Url(lyrics.Id), new AdminResolveSingleStreamingLinksRequest(SourceUrl));

        Client.ClearAuthentication();
        var publicResponse = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{lyrics.Slug}");

        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        string raw = await publicResponse.Content.ReadAsStringAsync();
        raw.Should().Contain("https://open.spotify.com/track/curated-abc");
    }
}
