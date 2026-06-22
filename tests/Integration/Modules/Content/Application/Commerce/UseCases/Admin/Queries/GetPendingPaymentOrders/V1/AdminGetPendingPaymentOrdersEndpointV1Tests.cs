using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders.V1;

/// <summary>
/// Integration tests for the AdminGetPendingPaymentOrders endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetPendingPaymentOrdersEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPendingPaymentOrders_AsSuperAdmin_ReturnsOnlyPendingPaymentOrders()
    {
        ContentOrderEntity pendingOrder = ContentOrderFactory.CreateSubmitted();
        CustomerEntity pendingCustomer = CustomerFactory.CreateWithId(pendingOrder.CustomerId);
        ContentOrderEntity draftOrder = ContentOrderFactory.Create();
        CustomerEntity draftCustomer = CustomerFactory.CreateWithId(draftOrder.CustomerId);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(pendingCustomer);
            ctx.Customers.Add(draftCustomer);
            ctx.ContentOrders.Add(pendingOrder);
            ctx.ContentOrders.Add(draftOrder);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Orders}/{CommerceRouteConstants.PendingPayment}?pageIndex=0&pageSize=50"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetPendingPaymentOrdersResponse>();
        body.Orders.Items.Should().OnlyContain(o => o.Status == EnumOrderStatus.PendingPayment);
        body.Orders.Items.Should().Contain(o => o.Id == pendingOrder.Id);
        body.Orders.Items.Should().NotContain(o => o.Id == draftOrder.Id);
    }

    [Fact]
    public async Task GetPendingPaymentOrders_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}/{CommerceRouteConstants.PendingPayment}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
