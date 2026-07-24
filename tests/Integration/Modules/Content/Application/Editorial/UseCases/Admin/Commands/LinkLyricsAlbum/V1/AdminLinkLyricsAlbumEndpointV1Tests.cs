using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsAlbum.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsAlbum.V1;

/// <summary>
/// Integration tests for the AdminLinkLyricsAlbum endpoint.
/// </summary>
[Collection("Database")]
public class AdminLinkLyricsAlbumEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<(LyricsEntity Lyrics, AlbumEntity Album)> SeedLyricsAndAlbumAsync()
    {
        return await SeedAsync<ContentDbContext, (LyricsEntity, AlbumEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);
            AlbumEntity album = AlbumFactory.Create();
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.Albums.Add(album);
            return (lyrics, album);
        });
    }

    [Fact]
    public async Task LinkLyricsAlbum_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(
                EditorialRouteConstants.Lyrics,
                Guid.NewGuid(),
                EditorialRouteConstants.Album
            ),
            new AdminLinkLyricsAlbumRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LinkLyricsAlbum_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(
                EditorialRouteConstants.Lyrics,
                Guid.NewGuid(),
                EditorialRouteConstants.Album
            ),
            new AdminLinkLyricsAlbumRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LinkLyricsAlbum_WithExistingAlbum_LinksAndPersists()
    {
        (LyricsEntity lyrics, AlbumEntity album) = await SeedLyricsAndAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Album),
            new AdminLinkLyricsAlbumRequest(album.Id)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminLinkLyricsAlbumResponse body = await response.ReadAsAsync<AdminLinkLyricsAlbumResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.AlbumId.Should().Be(album.Id);
    }

    /// <summary>
    /// Linking an album must never touch the plain-text <c>Album</c> field set via
    /// <c>UpdateMetadata</c> — unlinking should revert to that same free-text value untouched.
    /// </summary>
    [Fact]
    public async Task LinkLyricsAlbum_ShouldNotTouchPlainTextAlbumField()
    {
        (LyricsEntity lyrics, AlbumEntity album) = await SeedAsync<ContentDbContext, (LyricsEntity, AlbumEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity entity = LyricsFactory.Create(category.Id);
            entity.UpdateMetadata("Free Text Album", 1990, "Label", "Songwriter", "Producer");
            AlbumEntity album = AlbumFactory.Create();
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(entity);
            ctx.Albums.Add(album);
            return (entity, album);
        });

        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Album),
            new AdminLinkLyricsAlbumRequest(album.Id)
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.Album.Should().Be("Free Text Album");
    }

    [Fact]
    public async Task LinkLyricsAlbum_WithNonExistentAlbumId_ReturnsNotFound()
    {
        (LyricsEntity lyrics, _) = await SeedLyricsAndAlbumAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Album),
            new AdminLinkLyricsAlbumRequest(Guid.NewGuid())
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LinkLyricsAlbum_WithNullAlbumId_UnlinksExistingAlbum()
    {
        (LyricsEntity lyrics, AlbumEntity album) = await SeedLyricsAndAlbumAsync();
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Album),
            new AdminLinkLyricsAlbumRequest(album.Id)
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Album),
            new AdminLinkLyricsAlbumRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.AlbumId.Should().BeNull();
    }

    [Fact]
    public async Task LinkLyricsAlbum_ReLinkToDifferentAlbum_UpdatesToNewAlbum()
    {
        (LyricsEntity lyrics, AlbumEntity firstAlbum) = await SeedLyricsAndAlbumAsync();
        AlbumEntity secondAlbum = await SeedAsync<ContentDbContext, AlbumEntity>(ctx =>
        {
            AlbumEntity a = AlbumFactory.Create();
            ctx.Albums.Add(a);
            return a;
        });

        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Album),
            new AdminLinkLyricsAlbumRequest(firstAlbum.Id)
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Album),
            new AdminLinkLyricsAlbumRequest(secondAlbum.Id)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.AlbumId.Should().Be(secondAlbum.Id);
    }
}
