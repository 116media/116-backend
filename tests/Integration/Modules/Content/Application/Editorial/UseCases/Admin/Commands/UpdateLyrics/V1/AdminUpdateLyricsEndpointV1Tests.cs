using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;

/// <summary>
/// Integration tests for the AdminUpdateLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateLyrics_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();
        AdminUpdateLyricsRequest request = new AdminUpdateLyricsRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}", request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that updating an existing lyrics page returns 200 OK, echoes the new
    /// song title/artist/text in the typed response, and persists the changed fields.
    /// </summary>
    [Fact]
    public async Task UpdateLyrics_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity l = LyricsFactory.Create();
            ctx.Lyrics.Add(l);
            return l;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateLyricsRequest request = new AdminUpdateLyricsRequestBuilder()
            .WithSongTitle("Updated Song Title")
            .WithArtistName("Updated Artist")
            .WithLyricsText("Updated lyrics text that contains enough content to satisfy validation.")
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{lyrics.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateLyricsResponse>();
        body.Lyrics.Id.Should().Be(lyrics.Id);
        body.Lyrics.SongTitle.Should().Be(request.SongTitle);
        body.Lyrics.ArtistName.Should().Be(request.ArtistName);
        body.Lyrics.LyricsText.Should().Be(request.LyricsText);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().NotBeNull();
        persisted!.SongTitle.Should().Be(request.SongTitle);
        persisted.ArtistName.Should().Be(request.ArtistName);
        persisted.LyricsText.Should().Be(request.LyricsText);
    }
}
