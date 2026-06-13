namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed.V1;

/// <summary>
/// Integration tests for the PublicGetVideoPromotionFeed endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetVideoPromotionFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetVideoPromotionFeed_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/promotion/feed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
