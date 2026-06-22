using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetLyricsBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetLyricsBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetLyricsBySlug_WithExistingLyrics_ReturnsLyrics()
    {
        const string songTitle = "UniqueSlugSong";
        const string artistName = "UniqueSlugArtist";

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(songTitle, artistName);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{songTitle}/{artistName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.Lyrics.Id.Should().Be(lyrics.Id);
        body.Lyrics.SongTitle.Should().Be(songTitle);
        body.Lyrics.ArtistName.Should().Be(artistName);
    }

    [Fact]
    public async Task GetLyricsBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/non-existent-song/non-existent-artist");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
