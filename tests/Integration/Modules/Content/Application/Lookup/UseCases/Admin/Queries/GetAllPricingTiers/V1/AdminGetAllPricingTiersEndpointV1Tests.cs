using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers.V1;

/// <summary>
/// Integration tests for the AdminGetAllPricingTiers endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllPricingTiersEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllPricingTiers_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.PricingTiers);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllPricingTiers_AsAdmin_ReturnsSeededPricingTier()
    {
        PricingTierEntity pricingTier = await SeedAsync<ContentDbContext, PricingTierEntity>(ctx =>
        {
            PricingTierEntity entity = PricingTierFactory.Create();
            ctx.PricingTiers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.PricingTiers);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPricingTiersResponse>();
        body.PricingTiers.Should().Contain(t => t.Id == pricingTier.Id && t.Name == pricingTier.Name);
    }

    [Fact]
    public async Task GetAllPricingTiers_AsSuperAdmin_WithSearch_ReturnsMatchingPricingTiers()
    {
        PricingTierEntity matching = await SeedAsync<ContentDbContext, PricingTierEntity>(ctx =>
        {
            PricingTierEntity entity = PricingTierFactory.Create("base_upload");
            ctx.PricingTiers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.PricingTiers}?search=base");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllPricingTiersResponse>();
        body.PricingTiers.Should().Contain(t => t.Id == matching.Id);
        body.PricingTiers.Should()
            .OnlyContain(t =>
                t.Name.Contains("base", StringComparison.OrdinalIgnoreCase)
                || t.Description.Contains("base", StringComparison.OrdinalIgnoreCase)
            );
    }
}
