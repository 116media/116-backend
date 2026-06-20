namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetArticleBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArticleBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetArticleBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/non-existent-slug");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
