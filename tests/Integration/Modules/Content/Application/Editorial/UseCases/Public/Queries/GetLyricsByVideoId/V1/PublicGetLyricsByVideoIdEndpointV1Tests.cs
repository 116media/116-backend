namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId.V1;

/// <summary>
/// Integration tests for the PublicGetLyricsByVideoId endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetLyricsByVideoIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetLyricsByVideoId_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/videos/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
