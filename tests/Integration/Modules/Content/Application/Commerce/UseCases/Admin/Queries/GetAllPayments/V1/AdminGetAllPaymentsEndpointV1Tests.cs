using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments.V1;

/// <summary>
/// Integration tests for the AdminGetAllPayments endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllPaymentsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllPayments_AsSuperAdmin_ReturnsOk()
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

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Payments}?pageIndex=0&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetAllPaymentsResponse>();
        body.Payments.Items.Should().Contain(p => p.Id == payment.Id);

        PaymentSummaryDto dto = body.Payments.Items.Single(p => p.Id == payment.Id);
        dto.OrderId.Should().Be(order.Id);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.Status.Should().Be(EnumPaymentStatus.Pending);
    }

    [Fact]
    public async Task GetAllPayments_WithStatusFilter_ReturnsOnlyMatchingPayments()
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

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Payments}?pageIndex=0&pageSize=50&status={nameof(EnumPaymentStatus.Pending)}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetAllPaymentsResponse>();
        body.Payments.Items.Should().OnlyContain(p => p.Status == EnumPaymentStatus.Pending);
        body.Payments.Items.Should().Contain(p => p.Id == payment.Id);
    }

    [Fact]
    public async Task GetAllPayments_WithMethodFilter_ReturnsOnlyMatchingPayments()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(
            $"{ApiRoutes.Admin.Payments}?pageIndex=0&pageSize=50&method={nameof(EnumPaymentMethod.BankTransfer)}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetAllPaymentsResponse>();
        body.Payments.Items.Should().OnlyContain(p => p.PaymentMethod == EnumPaymentMethod.BankTransfer);
        body.Payments.Items.Should().Contain(p => p.Id == payment.Id);
    }

    [Fact]
    public async Task GetAllPayments_WithSearch_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Payments}?pageIndex=0&pageSize=10&search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await response.ReadAsAsync<AdminGetAllPaymentsResponse>();
    }

    [Fact]
    public async Task GetAllPayments_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Payments}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
