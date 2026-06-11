using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Content.Api.Commerce;

/// <summary>
/// Integration tests for the admin order-item endpoints verifying add, edit, remove
/// operations on order items and their pricing tiers against a real PostgreSQL database
/// through the full API pipeline.
/// </summary>
[Collection("Database")]
public class AdminOrderItemEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AddOrderItem_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.Customers.Add(customer);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            ContentKind = 0,
            CategoryId = category.Id.ToString(),
            SocialBoost = false,
            IsBonus = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Orders}/{order.Id}/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddOrderItem_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            ContentKind = 0,
            CategoryId = Guid.NewGuid().ToString(),
            SocialBoost = false,
            IsBonus = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddOrderItem_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            ContentKind = 0,
            CategoryId = Guid.NewGuid().ToString(),
            SocialBoost = false,
            IsBonus = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EditOrderItem_AsAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        var orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        seedContext.Customers.Add(customer);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.ContentOrders.Add(order);
        seedContext.ContentOrderItems.Add(orderItem);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new { ContentKind = 1, SocialBoost = true };

        var url = $"{ApiRoutes.Admin.Orders}/{order.Id}/items/{orderItem.Id}";
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EditOrderItem_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        var request = new { ContentKind = 1 };

        var url = $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}";
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EditOrderItem_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { ContentKind = 1 };

        var url = $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}";
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveOrderItem_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        var orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        seedContext.Customers.Add(customer);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.ContentOrders.Add(order);
        seedContext.ContentOrderItems.Add(orderItem);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Orders}/{order.Id}/items/{orderItem.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveOrderItem_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveOrderItem_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

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
