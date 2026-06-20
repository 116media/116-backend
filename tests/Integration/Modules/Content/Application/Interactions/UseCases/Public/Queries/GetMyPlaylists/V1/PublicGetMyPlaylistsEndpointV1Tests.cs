namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetMyPlaylists.V1;

/// <summary>
/// Integration tests for the PublicGetMyPlaylists endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetMyPlaylistsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetMyPlaylists_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyPlaylists_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.Playlists);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
