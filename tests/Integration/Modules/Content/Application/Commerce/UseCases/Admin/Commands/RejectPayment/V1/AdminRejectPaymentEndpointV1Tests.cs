using _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment.V1;

/// <summary>
/// Integration tests for the AdminRejectPayment endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectPaymentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RejectPayment_AsSuperAdmin_ReturnsOk()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { Notes = "Payment proof is unclear, please resubmit" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.RejectPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminRejectPaymentResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentPaymentEntity? persistedPayment = await db.ContentPayments.FindAsync(payment.Id);
        persistedPayment!.Status.Should().Be(EnumPaymentStatus.Rejected);

        ContentOrderEntity? persistedOrder = await db.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    [Fact]
    public async Task RejectPayment_WithUnknownOrderId_ReturnsOrderNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Notes = "Invalid payment" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.RejectPayment(Guid.NewGuid()))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task RejectPayment_WithOrderThatHasNoPayment_ReturnsPaymentNotFound()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { Notes = "Invalid payment" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.RejectPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentPayment"))
        );
    }

    [Fact]
    public async Task RejectPayment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Notes = "Invalid payment" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.RejectPayment(Guid.NewGuid()))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectPayment_WhenAlreadyRejected_ReturnsConflict()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateRejected(order.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { Notes = "Rejecting again" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.RejectPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.PaymentAlreadyRejected())
        );
    }

    [Fact]
    public async Task RejectPayment_WhenAlreadyVerified_ReturnsConflictAndLeavesThePaymentVerified()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(order.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { Notes = "Rejecting a payment that was already verified" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.RejectPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.PaymentAlreadyVerified())
        );

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        (await db.ContentPayments.FindAsync(payment.Id))!.Status.Should().Be(EnumPaymentStatus.Verified);
    }
}
