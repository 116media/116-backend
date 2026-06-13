namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos.V1;

/// <summary>
/// Integration tests for the PublicGetPromotedVideos endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPromotedVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPromotedVideos_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/promoted");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
