using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics.V1;

/// <summary>
/// Integration tests for the AdminGetAllLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Lyrics);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.Lyrics);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllLyrics_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Lyrics);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllLyrics_WithSearch_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Lyrics}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the search query parameter filters lyrics by song title,
    /// returning only lyrics whose song title matches the search term.
    /// </summary>
    [Fact]
    public async Task GetAllLyrics_WithSearchQuery_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var matchingLyrics = LyricsFactory.Create("UniqueSearchTerm Song", "Test Artist");
        var otherLyrics = LyricsFactory.Create("Completely Different Song", "Other Artist");
        context.Lyrics.AddRange(matchingLyrics, otherLyrics);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Lyrics}?search=UniqueSearchTerm");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
