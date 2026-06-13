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

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            SongTitle = "Eloko Oyo",
            ArtistName = "Fally Ipupa",
            LyricsText = "Eloko oyo ezali ya motema, na lingi yo mingi...",
            Language = "fr",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithEmptySongTitle_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            SongTitle = "",
            ArtistName = "Fally Ipupa",
            LyricsText = "Some lyrics text here.",
            Language = "fr",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithEmptyLyricsText_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            SongTitle = "Eloko Oyo",
            ArtistName = "Fally Ipupa",
            LyricsText = "",
            Language = "fr",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
