using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders.V1;

/// <summary>
/// Integration tests for the AdminGetCustomerOrders endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetCustomerOrdersEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetCustomerOrders_AsSuperAdmin_WithValidCustomer_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomerOrders_WithNonExistentCustomer_ReturnsOkOrNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}/orders");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomerOrders_WithInvalidId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/not-a-guid/orders");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomerOrders_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
