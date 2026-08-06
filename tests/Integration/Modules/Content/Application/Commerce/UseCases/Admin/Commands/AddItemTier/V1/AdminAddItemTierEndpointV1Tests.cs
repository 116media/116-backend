using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.V1;

/// <summary>
/// Integration tests for the AdminAddItemTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminAddItemTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AddItemTier_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        CategoryPricingEntity categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 9.99m);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PricingTiers.Add(pricingTier);
            ctx.CategoryPricing.Add(categoryPricing);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddItemTierRequest request = new AdminAddItemTierRequestBuilder()
            .WithPricingTierId(pricingTier.Id.ToString())
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Orders.ItemTiers(order.Id, orderItem.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminAddItemTierResponse>();
        body.Tier.Id.Should().NotBeEmpty();
        body.Tier.PriceSnapshotUsd.Should().Be(9.99m);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentItemTierEntity? persisted = await db.ContentItemTiers.FindAsync(body.Tier.Id);
        persisted.Should().NotBeNull();
        persisted!.OrderItemId.Should().Be(orderItem.Id);
        persisted.PricingTierId.Should().Be(pricingTier.Id);
        persisted.PriceSnapshotUsd.Should().Be(9.99m);
    }

    [Fact]
    public async Task AddItemTier_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminAddItemTierRequest request = new AdminAddItemTierRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Orders.ItemTiers(Guid.NewGuid(), Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task AddItemTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        AdminAddItemTierRequest request = new AdminAddItemTierRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Orders.ItemTiers(Guid.NewGuid(), Guid.NewGuid()),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddItemTier_WhenCategoryHasNoPricingForTier_ReturnsCategoryPricingNotFound()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PricingTiers.Add(pricingTier);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddItemTierRequest request = new AdminAddItemTierRequestBuilder()
            .WithPricingTierId(pricingTier.Id.ToString())
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Orders.ItemTiers(order.Id, orderItem.Id), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("CategoryPricing"))
        );

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        (await db.ContentItemTiers.AnyAsync(t => t.OrderItemId == orderItem.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task AddItemTier_WhenAlreadyAttached_ReturnsConflict()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        CategoryPricingEntity categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 9.99m);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        ContentItemTierEntity existingTier = ContentItemTierFactory.CreateDefault(orderItem.Id, pricingTier.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PricingTiers.Add(pricingTier);
            ctx.CategoryPricing.Add(categoryPricing);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
            ctx.ContentItemTiers.Add(existingTier);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddItemTierRequest request = new AdminAddItemTierRequestBuilder()
            .WithPricingTierId(pricingTier.Id.ToString())
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Orders.ItemTiers(order.Id, orderItem.Id), request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.TierAlreadyAttached())
        );
    }
}
