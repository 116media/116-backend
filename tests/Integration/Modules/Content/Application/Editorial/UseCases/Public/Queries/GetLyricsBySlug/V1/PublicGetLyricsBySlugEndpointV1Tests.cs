namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetLyricsBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetLyricsBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetLyricsBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/non-existent-song/non-existent-artist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
