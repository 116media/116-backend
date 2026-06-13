using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.V1;

/// <summary>
/// Integration tests for the AdminSubmitOrder endpoint.
/// </summary>
[Collection("Database")]
public class AdminSubmitOrderEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SubmitOrder_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var pricingTier = PricingTierFactory.Create();
        var categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 9.99m);
        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        var orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        var itemTier = ContentItemTierFactory.Create(orderItem.Id, pricingTier.Id, 9.99m);
        seedContext.Customers.Add(customer);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.PricingTiers.Add(pricingTier);
        seedContext.CategoryPricing.Add(categoryPricing);
        seedContext.ContentOrders.Add(order);
        seedContext.ContentOrderItems.Add(orderItem);
        seedContext.ContentItemTiers.Add(itemTier);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Orders}/{order.Id}/submit", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SubmitOrder_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/submit", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitOrder_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/submit", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
