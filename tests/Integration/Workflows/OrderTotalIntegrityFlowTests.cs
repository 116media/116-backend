using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Flow covering the order-total defect Stage 6 closes: every added item must land in the total,
/// so the payment created at submission freezes the amount the customer actually owes.
/// </summary>
[Collection("Database")]
public class OrderTotalIntegrityFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Submit_AfterAddingATieredItemAndAPromoOnlyItem_FreezesTheFullTotalIntoThePayment()
    {
        // Arrange — a draft order plus the catalog rows the flow needs
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        CategoryPricingEntity categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 100m);
        PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create("Homepage", 7, 200m);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PricingTiers.Add(pricingTier);
            ctx.CategoryPricing.Add(categoryPricing);
            ctx.PromotionLevels.Add(promotionLevel);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();

        // Act — item A with a tier, then item B carrying only a promotion price, then submit.
        // Before the fix, adding B never recalculated and the payment froze a total without it.
        var itemARequest = new AdminAddOrderItemRequestBuilder()
            .WithContentKind(EnumCoreContentType.Article)
            .WithCategoryId(category.Id.ToString())
            .Build();
        var itemAResponse = await Client.PostAsJsonAsync(Routes.Admin.Orders.Items(order.Id), itemARequest);
        itemAResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid itemAId = (await itemAResponse.ReadAsAsync<AdminAddOrderItemResponse>()).Item.Id;

        var tierRequest = new AdminAddItemTierRequestBuilder().WithPricingTierId(pricingTier.Id.ToString()).Build();
        var tierResponse = await Client.PostAsJsonAsync(Routes.Admin.Orders.ItemTiers(order.Id, itemAId), tierRequest);
        tierResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var itemBRequest = new AdminAddOrderItemRequestBuilder()
            .WithContentKind(EnumCoreContentType.Video)
            .WithCategoryId(category.Id.ToString())
            .WithPromotionLevelId(promotionLevel.Id)
            .Build();
        var itemBResponse = await Client.PostAsJsonAsync(Routes.Admin.Orders.Items(order.Id), itemBRequest);
        itemBResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        Guid itemBId = (await itemBResponse.ReadAsAsync<AdminAddOrderItemResponse>()).Item.Id;

        // Every item needs a tier to submit, so B gets one too; its promo price must also survive
        var tierBResponse = await Client.PostAsJsonAsync(Routes.Admin.Orders.ItemTiers(order.Id, itemBId), tierRequest);
        tierBResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var submitResponse = await Client.PatchAsync(Routes.Admin.Orders.Submit(order.Id), null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — the frozen payment covers both tiers and the promotion price
        decimal expectedTotal = 100m + 100m + 200m;

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        ContentOrderEntity persistedOrder = await verifyDb.ContentOrders.FirstAsync(o => o.Id == order.Id);
        persistedOrder.TotalAmountUsd.Should().Be(expectedTotal);

        ContentPaymentEntity payment = await verifyDb.ContentPayments.FirstAsync(p => p.OrderId == order.Id);
        payment.AmountUsd.Should().Be(expectedTotal);
    }

    [Fact]
    public async Task Submit_WhenAnyItemHasNoTier_IsRefused()
    {
        // Arrange — one tiered item and one without; the old Any guard let this through
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);

        ContentOrderItemEntity tiered = ContentOrderItemFactory.Create(order.Id, category.Id);
        tiered.Tiers.Add(ContentItemTierFactory.Create(tiered.Id, pricingTier.Id, 100m));
        ContentOrderItemEntity tierless = ContentOrderItemFactory.Create(order.Id, category.Id);

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PricingTiers.Add(pricingTier);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.AddRange(tiered, tierless);
        });

        Client.AuthenticateAsSuperAdmin();

        // Act
        var response = await Client.PatchAsync(Routes.Admin.Orders.Submit(order.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        ContentOrderEntity persisted = await verifyDb.ContentOrders.FirstAsync(o => o.Id == order.Id);
        persisted.Status.Should().Be(EnumOrderStatus.Draft);
    }
}
