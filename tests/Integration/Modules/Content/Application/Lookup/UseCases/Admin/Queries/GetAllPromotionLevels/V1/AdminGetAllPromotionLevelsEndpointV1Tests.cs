namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels.V1;

/// <summary>
/// Integration tests for the AdminGetAllPromotionLevels endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllPromotionLevelsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllPromotionLevels_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.PromotionLevels);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllPromotionLevels_AsAdmin_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.PromotionLevels);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllPromotionLevels_AsSuperAdmin_WithSearch_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.PromotionLevels}?search=featured");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
