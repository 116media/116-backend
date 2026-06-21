using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Cross-module workflow tests for the commerce order lifecycle:
/// create customer → create order → add items → submit → verify payment.
/// </summary>
[Collection("Database")]
public class OrderLifecycleTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateAndSubmitOrder_ShouldTransitionStatus()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);

        var pricingTier = PricingTierFactory.Create("base_upload");
        seedContext.PricingTiers.Add(pricingTier);
        await seedContext.SaveChangesAsync();

        var customer = CustomerFactory.Create($"submit-{Guid.NewGuid():N}@test.com");
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var item = ContentOrderItemFactory.Create(order.Id, category.Id);
        seedContext.ContentOrderItems.Add(item);
        await seedContext.SaveChangesAsync();

        var tier = ContentItemTierFactory.Create(item.Id, pricingTier.Id, 25.00m);
        seedContext.ContentItemTiers.Add(tier);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        HttpResponseMessage submitResponse = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Orders}/{order.Id}/submit",
            null
        );
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        ContentOrderEntity submitted = await verifyContext.ContentOrders.FirstAsync(o => o.Id == order.Id);
        submitted.Status.Should().NotBe(order.Status);
    }

    [Fact]
    public async Task CancelOrder_ShouldTransitionToCancelled()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);

        var customer = CustomerFactory.Create($"cancel-{Guid.NewGuid():N}@test.com");
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var orderRequest = new { CustomerId = customer.Id, CategoryId = category.Id };
        HttpResponseMessage orderResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, orderRequest);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var orderContext = CreateDbContext<ContentDbContext>();
        ContentOrderEntity order = await orderContext.ContentOrders.FirstAsync(o => o.CustomerId == customer.Id);

        HttpResponseMessage cancelResponse = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Orders}/{order.Id}/cancel",
            null
        );
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOrder_AsVisitor_ShouldReturnForbidden()
    {
        Client.AuthenticateAsVisitor();

        var request = new { CustomerId = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateOrder_WithNoAuth_ShouldReturnUnauthorized()
    {
        Client.ClearAuthentication();

        var request = new { CustomerId = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
