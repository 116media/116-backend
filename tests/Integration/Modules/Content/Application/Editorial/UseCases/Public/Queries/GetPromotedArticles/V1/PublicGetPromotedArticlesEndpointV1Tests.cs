namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles.V1;

/// <summary>
/// Integration tests for the PublicGetPromotedArticles endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPromotedArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPromotedArticles_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/promoted");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
