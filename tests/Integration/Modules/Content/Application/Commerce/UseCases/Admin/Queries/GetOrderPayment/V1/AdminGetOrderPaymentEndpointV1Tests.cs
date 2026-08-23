using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment.V1;

/// <summary>
/// Integration tests for the AdminGetOrderPayment endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOrderPaymentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetOrderPayment_AsSuperAdmin_WithSeededPayment_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(Routes.Admin.Orders.Payment(order.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetOrderPaymentResponse>();
        body.Payment.Id.Should().Be(payment.Id);
        body.Payment.Status.Should().Be(EnumPaymentStatus.Pending);
        body.Payment.AmountUsd.Should().Be(payment.AmountUsd);
    }

    [Fact]
    public async Task GetOrderPayment_WithUnknownOrderId_ReturnsOrderNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(Routes.Admin.Orders.Payment(Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task GetOrderPayment_WithOrderThatHasNoPayment_ReturnsPaymentNotFound()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(Routes.Admin.Orders.Payment(order.Id));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentPayment"))
        );
    }

    [Fact]
    public async Task GetOrderPayment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Admin.Orders.Payment(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
