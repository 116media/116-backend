using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata.V1;

/// <summary>
/// Integration tests for the AdminUpdateLyricsMetadata endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateLyricsMetadataEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<LyricsEntity> SeedLyricsAsync()
    {
        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });
    }

    [Fact]
    public async Task UpdateLyricsMetadata_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateLyricsMetadata_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateLyricsMetadata_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new AdminUpdateLyricsMetadataRequest(null, null, null, null, null)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies a full round trip: all five song-credit fields are set, returned in the
    /// response, and persisted.
    /// </summary>
    [Fact]
    public async Task UpdateLyricsMetadata_AsSuperAdmin_WithAllFields_PersistsMetadata()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var request = new AdminUpdateLyricsMetadataRequest(
            Album: "Testament",
            ReleaseYear: 1995,
            Label: "Sonodisc",
            Songwriter: "Papa Wemba",
            Producer: "Viviane Arnoux"
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateLyricsMetadataResponse>();
        body.Lyrics.Album.Should().Be("Testament");
        body.Lyrics.ReleaseYear.Should().Be(1995);
        body.Lyrics.Label.Should().Be("Sonodisc");
        body.Lyrics.Songwriter.Should().Be("Papa Wemba");
        body.Lyrics.Producer.Should().Be("Viviane Arnoux");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().NotBeNull();
        persisted!.Album.Should().Be("Testament");
        persisted.ReleaseYear.Should().Be(1995);
        persisted.Label.Should().Be("Sonodisc");
        persisted.Songwriter.Should().Be("Papa Wemba");
        persisted.Producer.Should().Be("Viviane Arnoux");
    }

    /// <summary>
    /// Each song-credit field is independently nullable/clearable — updating with only some
    /// fields set to null clears just those fields, leaving the others as previously persisted.
    /// </summary>
    [Fact]
    public async Task UpdateLyricsMetadata_WithSomeFieldsNulled_ClearsOnlyThoseFields()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminUpdateLyricsMetadataRequest("Album", 1990, "Label", "Songwriter", "Producer")
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminUpdateLyricsMetadataRequest(null, 1990, null, "Songwriter", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.Album.Should().BeNull();
        persisted.ReleaseYear.Should().Be(1990);
        persisted.Label.Should().BeNull();
        persisted.Songwriter.Should().Be("Songwriter");
        persisted.Producer.Should().BeNull();
    }

    /// <summary>
    /// An out-of-range release year is rejected with a validation problem end-to-end.
    /// </summary>
    [Fact]
    public async Task UpdateLyricsMetadata_WithReleaseYearOutOfBounds_ReturnsValidationProblem()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminUpdateLyricsMetadataRequest(null, 1899, null, null, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
