using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier.V1;

/// <summary>
/// Integration tests for the AdminRemoveItemTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveItemTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RemoveItemTier_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        var orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        var pricingTier = PricingTierFactory.Create();
        var itemTier = ContentItemTierFactory.Create(orderItem.Id, pricingTier.Id, 25.00m);
        seedContext.Customers.Add(customer);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.ContentOrders.Add(order);
        seedContext.ContentOrderItems.Add(orderItem);
        seedContext.PricingTiers.Add(pricingTier);
        seedContext.ContentItemTiers.Add(itemTier);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Orders}/{order.Id}/items/{orderItem.Id}/tiers/{itemTier.Id}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveItemTier_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}/tiers/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItemTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}/tiers/{Guid.NewGuid()}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
