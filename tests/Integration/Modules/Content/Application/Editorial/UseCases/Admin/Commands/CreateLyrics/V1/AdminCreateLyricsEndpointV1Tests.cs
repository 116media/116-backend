using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;

/// <summary>
/// Integration tests for the AdminCreateLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that creating a standalone lyrics page returns 201 Created, echoes the
    /// supplied fields in the typed response, and persists the lyrics row.
    /// </summary>
    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateLyricsResponse>();
        body.Lyrics.Id.Should().NotBeEmpty();
        body.Lyrics.SongTitle.Should().Be(request.SongTitle);
        body.Lyrics.ArtistName.Should().Be(request.ArtistName);
        body.Lyrics.Language.Should().Be(request.Language);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(body.Lyrics.Id);
        persisted.Should().NotBeNull();
        persisted!.SongTitle.Should().Be(request.SongTitle);
        persisted.ArtistName.Should().Be(request.ArtistName);
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithEmptySongTitle_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder().WithSongTitle(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithEmptyLyricsText_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder().WithLyricsText(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that creating lyrics with a song title and artist name combination
    /// that already exists returns a 409 Conflict problem.
    /// </summary>
    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithDuplicateSongAndArtist_ReturnsConflict()
    {
        await SeedAsync<ContentDbContext>(ctx => ctx.Lyrics.Add(LyricsFactory.Create("Nakombela", "Fally Ipupa")));

        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithSongTitle("Nakombela")
            .WithArtistName("Fally Ipupa")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }
}
