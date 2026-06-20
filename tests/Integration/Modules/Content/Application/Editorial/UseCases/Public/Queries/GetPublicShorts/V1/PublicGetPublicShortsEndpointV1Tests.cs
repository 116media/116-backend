namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts.V1;

/// <summary>
/// Integration tests for the PublicGetPublicShorts endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublicShortsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetShorts_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
