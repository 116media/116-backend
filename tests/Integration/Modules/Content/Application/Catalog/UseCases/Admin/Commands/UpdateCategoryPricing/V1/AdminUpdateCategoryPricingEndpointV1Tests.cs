using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing.V1;

/// <summary>
/// Integration tests for the AdminUpdateCategoryPricing endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateCategoryPricingEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task UpdateCategoryPricing_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        var pricingTier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(pricingTier);
        var categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 5.99m);
        seedContext.CategoryPricing.Add(categoryPricing);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{category.Id}/pricing/{pricingTier.Id}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateCategoryPricing_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategoryPricing_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategoryPricing_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { PriceUsd = 12.99m };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/pricing/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
