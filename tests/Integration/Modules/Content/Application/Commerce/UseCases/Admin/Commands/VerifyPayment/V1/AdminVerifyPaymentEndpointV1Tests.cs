using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;

/// <summary>
/// Integration tests for the AdminVerifyPayment endpoint.
/// </summary>
[Collection("Database")]
public class AdminVerifyPaymentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task VerifyPayment_AsSuperAdmin_ReturnsOk()
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
        var request = new { ReceiptUrl = _116.Tests.Fixtures.Constants.TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminVerifyPaymentResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentPaymentEntity? persistedPayment = await db.ContentPayments.FindAsync(payment.Id);
        persistedPayment!.Status.Should().Be(EnumPaymentStatus.Verified);

        ContentOrderEntity? persistedOrder = await db.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.Paid);
    }

    [Fact]
    public async Task VerifyPayment_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = _116.Tests.Fixtures.Constants.TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(Guid.NewGuid()))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyPayment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { ReceiptUrl = _116.Tests.Fixtures.Constants.TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(Guid.NewGuid()))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that verifying an already-verified payment returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task VerifyPayment_WhenAlreadyVerified_ReturnsConflict()
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
        var request = new { ReceiptUrl = _116.Tests.Fixtures.Constants.TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that verifying payment on an order with a video order item succeeds,
    /// covering the VideoByOrderItemIdSpecification lookup path.
    /// </summary>
    [Fact]
    public async Task VerifyPayment_WithVideoOrderItem_ReturnsOk()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        VideoEntity video = VideoFactory.CreatePaid(category.Id, customer.Id, orderItem.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateWithProof(order.Id, Guid.NewGuid());
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.Videos.Add(video);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = _116.Tests.Fixtures.Constants.TestConstants.Content.Commerce.ValidReceiptUrl };
        var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.VerifyPayment(order.Id))
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminVerifyPaymentResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentPaymentEntity? persistedPayment = await db.ContentPayments.FindAsync(payment.Id);
        persistedPayment!.Status.Should().Be(EnumPaymentStatus.Verified);

        ContentOrderEntity? persistedOrder = await db.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.Paid);
    }
}
