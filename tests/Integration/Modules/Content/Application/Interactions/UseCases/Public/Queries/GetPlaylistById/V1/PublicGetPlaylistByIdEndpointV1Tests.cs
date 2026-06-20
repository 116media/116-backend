namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetPlaylistById.V1;

/// <summary>
/// Integration tests for the PublicGetPlaylistById endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPlaylistByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPlaylistById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPlaylistById_AsVisitor_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Playlists}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
