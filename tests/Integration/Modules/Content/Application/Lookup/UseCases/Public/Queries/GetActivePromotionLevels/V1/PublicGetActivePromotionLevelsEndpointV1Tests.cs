namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels.V1;

/// <summary>
/// Integration tests for the PublicGetActivePromotionLevels endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetActivePromotionLevelsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetActivePromotionLevels_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.PromotionLevels);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActivePromotionLevels_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.PromotionLevels);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
