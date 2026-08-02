using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink.V1;

/// <summary>
/// Integration tests for the AdminUpsertSingleStreamingLink endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpsertSingleStreamingLinkEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string SpotifyUrl = "https://open.spotify.com/track/first-curated";
    private const string ReplacementUrl = "https://open.spotify.com/track/second-curated";

    /// <summary>
    /// Builds the single streaming link route for the given lyrics page and platform.
    /// </summary>
    /// <param name="lyricsId">The standalone single the link belongs to.</param>
    /// <param name="platform">The streaming platform slot.</param>
    /// <returns>The fully qualified endpoint URL.</returns>
    private static string Url(Guid lyricsId, EnumStreamingPlatform platform) =>
        Routes.Admin.Editorial.StreamingLink(EditorialRouteConstants.Lyrics, lyricsId, platform.ToString());

    /// <summary>
    /// Seeds the content type and category rows a lyrics page depends on.
    /// </summary>
    /// <returns>The seeded category identifier.</returns>
    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });
    }

    /// <summary>
    /// Seeds a standalone single — a lyrics page with no owning album.
    /// </summary>
    /// <returns>The seeded lyrics page.</returns>
    private async Task<LyricsEntity> SeedSingleAsync()
    {
        Guid categoryId = await SeedCategoryAsync();

        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity lyrics = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });
    }

    [Fact]
    public async Task UpsertSingleStreamingLink_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify),
            new AdminUpsertSingleStreamingLinkRequest(SpotifyUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpsertSingleStreamingLink_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify),
            new AdminUpsertSingleStreamingLinkRequest(SpotifyUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpsertSingleStreamingLink_AsAdmin_WithNonExistentLyrics_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumStreamingPlatform.Spotify),
            new AdminUpsertSingleStreamingLinkRequest(SpotifyUrl)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// The first upsert for an empty platform slot inserts a new curated link row bound to the
    /// standalone single, leaving the album-side association unset.
    /// </summary>
    [Fact]
    public async Task UpsertSingleStreamingLink_WhenSlotEmpty_CreatesLinkAndPersists()
    {
        LyricsEntity lyrics = await SeedSingleAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(lyrics.Id, EnumStreamingPlatform.Spotify),
            new AdminUpsertSingleStreamingLinkRequest(SpotifyUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpsertSingleStreamingLinkResponse body =
            await response.ReadAsAsync<AdminUpsertSingleStreamingLinkResponse>();
        body.StreamingLinkId.Should().NotBeEmpty();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        StreamingLinkEntity? persisted = await ctx.StreamingLinks.FirstOrDefaultAsync(link =>
            link.Id == body.StreamingLinkId
        );

        persisted.Should().NotBeNull();
        persisted!.LyricsId.Should().Be(lyrics.Id);
        persisted.AlbumId.Should().BeNull();
        persisted.Platform.Should().Be(EnumStreamingPlatform.Spotify);
        persisted.Url.Should().Be(SpotifyUrl);
    }

    /// <summary>
    /// A second upsert on an already-populated platform slot replaces the curated URL on the
    /// existing row instead of inserting a duplicate.
    /// </summary>
    [Fact]
    public async Task UpsertSingleStreamingLink_WhenSlotAlreadySet_ReplacesUrlWithoutDuplicating()
    {
        LyricsEntity lyrics = await SeedSingleAsync();
        Client.AuthenticateAsAdmin();

        var firstResponse = await Client.PutAsJsonAsync(
            Url(lyrics.Id, EnumStreamingPlatform.Spotify),
            new AdminUpsertSingleStreamingLinkRequest(SpotifyUrl)
        );
        AdminUpsertSingleStreamingLinkResponse firstBody =
            await firstResponse.ReadAsAsync<AdminUpsertSingleStreamingLinkResponse>();

        var response = await Client.PutAsJsonAsync(
            Url(lyrics.Id, EnumStreamingPlatform.Spotify),
            new AdminUpsertSingleStreamingLinkRequest(ReplacementUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpsertSingleStreamingLinkResponse body =
            await response.ReadAsAsync<AdminUpsertSingleStreamingLinkResponse>();
        body.StreamingLinkId.Should().Be(firstBody.StreamingLinkId);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<StreamingLinkEntity> persisted = await ctx
            .StreamingLinks.Where(link => link.LyricsId == lyrics.Id)
            .ToListAsync();

        persisted.Should().ContainSingle();
        persisted[0].Url.Should().Be(ReplacementUrl);
    }

    /// <summary>
    /// A track that belongs to an album gets its streaming links through the album, so the
    /// per-track upsert is rejected as a conflict and nothing is persisted.
    /// </summary>
    [Fact]
    public async Task UpsertSingleStreamingLink_WhenLyricsBelongsToAlbum_ReturnsConflict()
    {
        Guid categoryId = await SeedCategoryAsync();
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            AlbumEntity album = AlbumFactory.Create();
            ctx.Albums.Add(album);

            LyricsEntity entity = LyricsFactory.CreateForAlbum(categoryId, album.Id);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(lyrics.Id, EnumStreamingPlatform.Spotify),
            new AdminUpsertSingleStreamingLinkRequest(SpotifyUrl)
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        bool anyPersisted = await ctx.StreamingLinks.AnyAsync(link => link.LyricsId == lyrics.Id);

        anyPersisted.Should().BeFalse();
    }
}
