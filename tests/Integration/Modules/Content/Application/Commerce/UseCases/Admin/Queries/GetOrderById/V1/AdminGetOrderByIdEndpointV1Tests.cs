using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById.V1;

/// <summary>
/// Integration tests for the AdminGetOrderById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOrderByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetOrderById_AsSuperAdmin_WithSeededOrder_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}/{order.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOrderById_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
