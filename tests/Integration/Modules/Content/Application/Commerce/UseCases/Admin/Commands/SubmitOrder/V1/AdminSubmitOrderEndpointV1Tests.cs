using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.V1;

/// <summary>
/// Integration tests for the AdminSubmitOrder endpoint.
/// </summary>
[Collection("Database")]
public class AdminSubmitOrderEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SubmitOrder_AsSuperAdmin_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        CategoryPricingEntity categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 9.99m);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        ContentItemTierEntity itemTier = ContentItemTierFactory.Create(orderItem.Id, pricingTier.Id, 9.99m);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PricingTiers.Add(pricingTier);
            ctx.CategoryPricing.Add(categoryPricing);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.ContentItemTiers.Add(itemTier);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Submit(order.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminSubmitOrderResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persistedOrder = await db.ContentOrders.FindAsync(order.Id);
        persistedOrder!.Status.Should().Be(EnumOrderStatus.PendingPayment);

        ContentPaymentEntity? persistedPayment = await db.ContentPayments.FirstOrDefaultAsync(p =>
            p.OrderId == order.Id
        );
        persistedPayment.Should().NotBeNull();
        persistedPayment!.Status.Should().Be(EnumPaymentStatus.Pending);
    }

    [Fact]
    public async Task SubmitOrder_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Submit(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task SubmitOrder_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Submit(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitOrder_AsSuperAdmin_WhenPendingPaymentWithPricedItem_ReturnsAlreadySubmittedConflict()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        ContentItemTierEntity itemTier = ContentItemTierFactory.Create(orderItem.Id, pricingTier.Id, 9.99m);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PricingTiers.Add(pricingTier);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.ContentItemTiers.Add(itemTier);
        });

        Client.AuthenticateAsSuperAdmin();
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var response = await Client.PatchAsync(Routes.Admin.Orders.Submit(order.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.AlreadySubmitted(), LocalizedMessage.EnglishCulture)
        );

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(order.Id);
        persisted!.Status.Should().Be(EnumOrderStatus.PendingPayment);

        // No second payment record is created when the status guard rejects the resubmission.
        (await db.ContentPayments.CountAsync(p => p.OrderId == order.Id))
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task SubmitOrder_WithItemButNoTiers_ReturnsBadRequest()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Submit(order.Id), null);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ContentOrderErrorMessage>(m => m.MustHaveAtLeastOneItemWithTier())
        );

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(order.Id);
        persisted!.Status.Should().Be(EnumOrderStatus.Draft);
    }
}
