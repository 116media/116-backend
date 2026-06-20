namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;

/// <summary>
/// Integration tests for the PublicGetPublishedVideos endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublishedVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPublishedVideos_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Videos);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
