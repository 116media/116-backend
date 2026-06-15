using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder.V1;

/// <summary>
/// Integration tests for the AdminCancelOrder endpoint.
/// </summary>
[Collection("Database")]
public class AdminCancelOrderEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CancelOrder_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Orders}/{order.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CancelOrder_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelOrder_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelOrder_AsSuperAdmin_AlreadyCancelled_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var order = ContentOrderFactory.CreateCancelled();
        var customer = CustomerFactory.CreateWithId(order.CustomerId);
        seedContext.Customers.Add(customer);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Orders}/{order.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelOrder_AsSuperAdmin_PaidOrder_ReturnsBadRequest()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var order = ContentOrderFactory.CreatePaid();
        var customer = CustomerFactory.CreateWithId(order.CustomerId);
        seedContext.Customers.Add(customer);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Orders}/{order.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
