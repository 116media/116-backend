using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier.V1;

/// <summary>
/// Integration tests for the AdminDeactivatePricingTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivatePricingTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeactivatePricingTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PricingTiers}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivatePricingTier_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PricingTiers}/{Guid.NewGuid()}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivatePricingTier_AsSuperAdmin_WithValidId_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var pricingTier = PricingTierFactory.Create();
        context.PricingTiers.Add(pricingTier);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.PricingTiers}/{pricingTier.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
