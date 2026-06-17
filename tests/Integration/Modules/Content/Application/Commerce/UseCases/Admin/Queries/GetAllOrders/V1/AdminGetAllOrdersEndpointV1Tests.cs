using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders.V1;

/// <summary>
/// Integration tests for the AdminGetAllOrders endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllOrdersEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllOrders_AsSuperAdmin_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetAllOrdersResponse>();
        body.Orders.Items.Should().Contain(o => o.Id == order.Id);

        ContentOrderSummaryDto dto = body.Orders.Items.Single(o => o.Id == order.Id);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.Status.Should().Be(EnumOrderStatus.Draft);
        dto.ItemCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllOrders_AsAdmin_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetAllOrdersResponse>();
        body.Orders.PageIndex.Should().Be(0);
        body.Orders.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllOrders_WithStatusFilter_ReturnsOnlyMatchingOrders()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity draftOrder = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderEntity cancelledOrder = ContentOrderFactory.CreateCancelled();
        CustomerEntity cancelledCustomer = CustomerFactory.CreateWithId(cancelledOrder.CustomerId);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.Customers.Add(cancelledCustomer);
            ctx.ContentOrders.Add(draftOrder);
            ctx.ContentOrders.Add(cancelledOrder);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Orders}?pageIndex=0&pageSize=50&status={nameof(EnumOrderStatus.Cancelled)}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetAllOrdersResponse>();
        body.Orders.Items.Should().OnlyContain(o => o.Status == EnumOrderStatus.Cancelled);
        body.Orders.Items.Should().Contain(o => o.Id == cancelledOrder.Id);
        body.Orders.Items.Should().NotContain(o => o.Id == draftOrder.Id);
    }

    [Fact]
    public async Task GetAllOrders_WithSearch_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create($"search-{Guid.NewGuid():N}@test.com");
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}?pageIndex=0&pageSize=10&search=search");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await response.ReadAsAsync<AdminGetAllOrdersResponse>();
    }

    [Fact]
    public async Task GetAllOrders_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
