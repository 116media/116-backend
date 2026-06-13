namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetVideoBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetVideoBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetVideoBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/non-existent-slug");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
