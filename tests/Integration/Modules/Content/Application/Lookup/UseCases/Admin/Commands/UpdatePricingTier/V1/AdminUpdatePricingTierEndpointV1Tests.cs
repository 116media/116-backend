using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier.V1;

/// <summary>
/// Integration tests for the AdminUpdatePricingTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdatePricingTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdatePricingTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Updated", Description = "Updated desc" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePricingTier_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Updated", Description = "Updated desc" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePricingTier_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var pricingTier = PricingTierFactory.Create();
        context.PricingTiers.Add(pricingTier);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Updated Tier", Description = "Updated description" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{pricingTier.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
