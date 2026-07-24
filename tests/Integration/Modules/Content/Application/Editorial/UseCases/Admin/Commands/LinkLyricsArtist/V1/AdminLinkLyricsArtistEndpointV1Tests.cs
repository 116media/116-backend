using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist.V1;

/// <summary>
/// Integration tests for the AdminLinkLyricsArtist endpoint.
/// </summary>
[Collection("Database")]
public class AdminLinkLyricsArtistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<(LyricsEntity Lyrics, ArtistEntity Artist)> SeedLyricsAndArtistAsync()
    {
        return await SeedAsync<ContentDbContext, (LyricsEntity, ArtistEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);
            ArtistEntity artist = ArtistFactory.Create();
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.Artists.Add(artist);
            return (lyrics, artist);
        });
    }

    [Fact]
    public async Task LinkLyricsArtist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(
                EditorialRouteConstants.Lyrics,
                Guid.NewGuid(),
                EditorialRouteConstants.Artist
            ),
            new AdminLinkLyricsArtistRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LinkLyricsArtist_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(
                EditorialRouteConstants.Lyrics,
                Guid.NewGuid(),
                EditorialRouteConstants.Artist
            ),
            new AdminLinkLyricsArtistRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LinkLyricsArtist_AsAdmin_WithNonExistentLyricsId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(
                EditorialRouteConstants.Lyrics,
                Guid.NewGuid(),
                EditorialRouteConstants.Artist
            ),
            new AdminLinkLyricsArtistRequest(null)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LinkLyricsArtist_WithExistingArtist_LinksAndPersists()
    {
        (LyricsEntity lyrics, ArtistEntity artist) = await SeedLyricsAndArtistAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Artist),
            new AdminLinkLyricsArtistRequest(artist.Id)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminLinkLyricsArtistResponse body = await response.ReadAsAsync<AdminLinkLyricsArtistResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.ArtistId.Should().Be(artist.Id);
    }

    /// <summary>
    /// Linking an artist profile must never touch the plain-text <c>ArtistName</c> field —
    /// unlinking later should revert the record to that same untouched free-text value.
    /// </summary>
    [Fact]
    public async Task LinkLyricsArtist_ShouldNotTouchPlainTextArtistName()
    {
        (LyricsEntity lyrics, ArtistEntity artist) = await SeedLyricsAndArtistAsync();
        string originalArtistName = lyrics.ArtistName;
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Artist),
            new AdminLinkLyricsArtistRequest(artist.Id)
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.ArtistName.Should().Be(originalArtistName);
    }

    [Fact]
    public async Task LinkLyricsArtist_WithNonExistentArtistId_ReturnsNotFound()
    {
        (LyricsEntity lyrics, _) = await SeedLyricsAndArtistAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Artist),
            new AdminLinkLyricsArtistRequest(Guid.NewGuid())
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Unlinking (passing a null <c>ArtistId</c>) reverts the lyrics page to its plain-text
    /// <c>ArtistName</c> only, with no data loss.
    /// </summary>
    [Fact]
    public async Task LinkLyricsArtist_WithNullArtistId_UnlinksExistingArtist()
    {
        (LyricsEntity lyrics, ArtistEntity artist) = await SeedLyricsAndArtistAsync();
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Artist),
            new AdminLinkLyricsArtistRequest(artist.Id)
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Artist),
            new AdminLinkLyricsArtistRequest(null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.ArtistId.Should().BeNull();
    }

    /// <summary>
    /// Re-linking to a different artist after an unlink replaces the association cleanly.
    /// </summary>
    [Fact]
    public async Task LinkLyricsArtist_ReLinkToDifferentArtist_UpdatesToNewArtist()
    {
        (LyricsEntity lyrics, ArtistEntity firstArtist) = await SeedLyricsAndArtistAsync();
        ArtistEntity secondArtist = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity a = ArtistFactory.Create();
            ctx.Artists.Add(a);
            return a;
        });

        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Artist),
            new AdminLinkLyricsArtistRequest(firstArtist.Id)
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Action(EditorialRouteConstants.Lyrics, lyrics.Id, EditorialRouteConstants.Artist),
            new AdminLinkLyricsArtistRequest(secondArtist.Id)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.ArtistId.Should().Be(secondArtist.Id);
    }
}
