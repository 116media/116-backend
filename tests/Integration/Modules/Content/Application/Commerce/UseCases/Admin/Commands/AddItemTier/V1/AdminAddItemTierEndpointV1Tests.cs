using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.V1;

/// <summary>
/// Integration tests for the AdminAddItemTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminAddItemTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AddItemTier_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var pricingTier = PricingTierFactory.Create();
        var categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 9.99m);
        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        var orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        seedContext.Customers.Add(customer);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.PricingTiers.Add(pricingTier);
        seedContext.CategoryPricing.Add(categoryPricing);
        seedContext.ContentOrders.Add(order);
        seedContext.ContentOrderItems.Add(orderItem);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PricingTierId = pricingTier.Id.ToString() };

        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Orders}/{order.Id}/items/{orderItem.Id}/tiers",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddItemTier_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { PricingTierId = Guid.NewGuid().ToString() };

        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}/tiers",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItemTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { PricingTierId = Guid.NewGuid().ToString() };

        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}/tiers",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
