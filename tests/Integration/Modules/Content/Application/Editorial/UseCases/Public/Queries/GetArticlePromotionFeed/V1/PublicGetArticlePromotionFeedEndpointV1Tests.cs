namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed.V1;

/// <summary>
/// Integration tests for the PublicGetArticlePromotionFeed endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArticlePromotionFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetArticlePromotionFeed_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/promotion/feed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
