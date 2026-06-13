namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetPublicShortBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublicShortBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetShortBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/non-existent-slug");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
