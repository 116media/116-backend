namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles.V1;

/// <summary>
/// Integration tests for the PublicGetPublishedArticles endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublishedArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPublishedArticles_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Articles);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
