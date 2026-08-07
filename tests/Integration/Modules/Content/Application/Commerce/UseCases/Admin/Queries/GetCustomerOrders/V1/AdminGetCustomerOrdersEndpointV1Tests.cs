using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
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
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Customers}/{customer.Id}/{CommerceRouteConstants.Orders}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetCustomerOrdersResponse>();
        body.Orders.Items.Should().Contain(o => o.Id == order.Id);

        ContentOrderSummaryDto dto = body.Orders.Items.Single(o => o.Id == order.Id);
        dto.CustomerName.Should().Be(customer.FullName);
    }

    [Fact]
    public async Task GetCustomerOrders_WithNonExistentCustomer_ReturnsEmptyResult()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}/{CommerceRouteConstants.Orders}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetCustomerOrdersResponse>();
        body.Orders.Items.Should().BeEmpty();
        body.Orders.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetCustomerOrders_WithInvalidId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/not-a-guid/{CommerceRouteConstants.Orders}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomerOrders_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}/{CommerceRouteConstants.Orders}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
