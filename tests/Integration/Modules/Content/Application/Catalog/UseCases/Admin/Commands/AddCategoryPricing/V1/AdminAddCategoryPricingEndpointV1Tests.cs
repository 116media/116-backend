using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;

/// <summary>
/// Integration tests for the AdminAddCategoryPricing endpoint.
/// </summary>
[Collection("Database")]
public class AdminAddCategoryPricingEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task AddCategoryPricing_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        var pricingTier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(pricingTier);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PricingTierId = pricingTier.Id, PriceUsd = 9.99m };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/pricing", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var pricing = await verifyContext.CategoryPricing.FirstOrDefaultAsync(cp =>
            cp.CategoryId == category.Id && cp.PricingTierId == pricingTier.Id
        );
        pricing.Should().NotBeNull();
    }

    [Fact]
    public async Task AddCategoryPricing_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { PricingTierId = Guid.NewGuid(), PriceUsd = 4.99m };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddCategoryPricing_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { PricingTierId = Guid.NewGuid(), PriceUsd = 4.99m };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddCategoryPricing_NonExistentCategory_ReturnsNotFound()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var pricingTier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(pricingTier);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PricingTierId = pricingTier.Id, PriceUsd = 4.99m };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
