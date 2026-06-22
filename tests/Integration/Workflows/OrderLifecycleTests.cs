using _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder.V1;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Cross-module workflow tests for the commerce order lifecycle:
/// create customer → create order → add items → submit → verify payment.
/// </summary>
[Collection("Database")]
public class OrderLifecycleTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateAndSubmitOrder_ShouldTransitionStatus()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(context =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create("Video");
            context.ContentTypes.Add(entity);
            return entity;
        });

        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(context =>
        {
            CategoryEntity entity = CategoryFactory.Create(contentType.Id);
            context.Categories.Add(entity);
            return entity;
        });

        PricingTierEntity pricingTier = await SeedAsync<ContentDbContext, PricingTierEntity>(context =>
        {
            PricingTierEntity entity = PricingTierFactory.Create("base_upload");
            context.PricingTiers.Add(entity);
            return entity;
        });

        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(context =>
        {
            CustomerEntity entity = CustomerFactory.Create($"submit-{Guid.NewGuid():N}@test.com");
            context.Customers.Add(entity);
            return entity;
        });

        ContentOrderEntity order = await SeedAsync<ContentDbContext, ContentOrderEntity>(context =>
        {
            ContentOrderEntity entity = ContentOrderFactory.CreateForCustomer(customer.Id);
            context.ContentOrders.Add(entity);
            return entity;
        });

        ContentOrderItemEntity item = await SeedAsync<ContentDbContext, ContentOrderItemEntity>(context =>
        {
            ContentOrderItemEntity entity = ContentOrderItemFactory.Create(order.Id, category.Id);
            context.ContentOrderItems.Add(entity);
            return entity;
        });

        await SeedAsync<ContentDbContext>(context =>
            context.ContentItemTiers.Add(ContentItemTierFactory.Create(item.Id, pricingTier.Id, 25.00m))
        );

        order.Status.Should().Be(EnumOrderStatus.Draft);

        Client.AuthenticateAsSuperAdmin();

        HttpResponseMessage submitResponse = await Client.PatchAsync(Routes.Admin.Orders.Submit(order.Id), null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminSubmitOrderResponse submitBody = await submitResponse.ReadAsAsync<AdminSubmitOrderResponse>();
        submitBody.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ContentOrderEntity submitted = await verifyContext.ContentOrders.FirstAsync(o => o.Id == order.Id);
        submitted.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    [Fact]
    public async Task CancelOrder_ShouldTransitionToCancelled()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(context =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create("Video");
            context.ContentTypes.Add(entity);
            return entity;
        });

        await SeedAsync<ContentDbContext>(context => context.Categories.Add(CategoryFactory.Create(contentType.Id)));

        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(context =>
        {
            CustomerEntity entity = CustomerFactory.Create($"cancel-{Guid.NewGuid():N}@test.com");
            context.Customers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var orderRequest = new AdminCreateOrderRequest(CustomerId: customer.Id.ToString(), PackageId: null);
        HttpResponseMessage orderResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, orderRequest);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateOrderResponse createBody = await orderResponse.ReadAsAsync<AdminCreateOrderResponse>();
        Guid orderId = createBody.Order.Id;
        createBody.Order.Status.Should().Be(EnumOrderStatus.Draft);

        HttpResponseMessage cancelResponse = await Client.PatchAsync(Routes.Admin.Orders.Cancel(orderId), null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminCancelOrderResponse cancelBody = await cancelResponse.ReadAsAsync<AdminCancelOrderResponse>();
        cancelBody.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ContentOrderEntity cancelled = await verifyContext.ContentOrders.FirstAsync(o => o.Id == orderId);
        cancelled.Status.Should().Be(EnumOrderStatus.Cancelled);
    }

    [Fact]
    public async Task CreateOrder_AsVisitor_ShouldReturnForbidden()
    {
        Client.AuthenticateAsVisitor();

        var request = new AdminCreateOrderRequest(CustomerId: Guid.NewGuid().ToString(), PackageId: null);
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateOrder_WithNoAuth_ShouldReturnUnauthorized()
    {
        Client.ClearAuthentication();

        var request = new AdminCreateOrderRequest(CustomerId: Guid.NewGuid().ToString(), PackageId: null);
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
