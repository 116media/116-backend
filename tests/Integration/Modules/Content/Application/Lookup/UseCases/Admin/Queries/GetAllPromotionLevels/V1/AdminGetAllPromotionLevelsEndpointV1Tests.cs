using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

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
    public async Task GetAllPromotionLevels_AsAdmin_ReturnsSeededPromotionLevel()
    {
        PromotionLevelEntity promotionLevel = await SeedAsync<ContentDbContext, PromotionLevelEntity>(ctx =>
        {
            PromotionLevelEntity entity = PromotionLevelFactory.Create();
            ctx.PromotionLevels.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.PromotionLevels);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPromotionLevelsResponse>();
        body.PromotionLevels.Should().Contain(p => p.Id == promotionLevel.Id && p.Name == promotionLevel.Name);
    }

    [Fact]
    public async Task GetAllPromotionLevels_AsSuperAdmin_WithSearch_ReturnsMatchingPromotionLevels()
    {
        PromotionLevelEntity matching = await SeedAsync<ContentDbContext, PromotionLevelEntity>(ctx =>
        {
            PromotionLevelEntity entity = PromotionLevelFactory.Create("Featured Spot", 7, 50m);
            ctx.PromotionLevels.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.PromotionLevels}?search=featured");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPromotionLevelsResponse>();
        body.PromotionLevels.Should().Contain(p => p.Id == matching.Id);
        body.PromotionLevels.Should().OnlyContain(p => p.Name.Contains("featured", StringComparison.OrdinalIgnoreCase));
    }
}
