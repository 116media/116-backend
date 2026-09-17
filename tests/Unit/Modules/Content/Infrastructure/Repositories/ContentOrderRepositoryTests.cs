using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="ContentOrderRepository"/> using InMemory database.
/// </summary>
public class ContentOrderRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ContentOrderRepository _repository;

    public ContentOrderRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new CreatedAtStampingInterceptor())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new ContentOrderRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<CustomerEntity> SeedCustomerAsync()
    {
        CustomerEntity customer = CustomerFactory.Create();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    private async Task<ContentOrderEntity> SeedOrderAsync(Guid? customerId = null)
    {
        CustomerEntity customer = customerId.HasValue
            ? (await _context.Customers.FindAsync(customerId.Value))!
            : await SeedCustomerAsync();

        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await _repository.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    // InMemory drops Include rows whose required principal is missing, so items and tiers reference persisted rows.
    private async Task<CategoryEntity> SeedCategoryAsync()
    {
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    private async Task<PricingTierEntity> SeedPricingTierAsync()
    {
        PricingTierEntity pricingTier = PricingTierFactory.Create();
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();
        return pricingTier;
    }

    private async Task<(ContentOrderEntity Order, ContentOrderItemEntity Item)> SeedOrderWithItemAsync()
    {
        ContentOrderEntity order = await SeedOrderAsync();
        CategoryEntity category = await SeedCategoryAsync();

        ContentOrderItemEntity item = order.AddItem(
            contentKind: EnumCoreContentType.Article,
            categoryId: category.Id,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: false,
            isBonus: false
        );
        await _context.SaveChangesAsync();

        return (order, item);
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistOrderToDatabase()
    {
        // Arrange
        await SeedCustomerAsync();
        ContentOrderEntity order = ContentOrderFactory.Create();

        // Act
        await _repository.AddAsync(order);
        await _context.SaveChangesAsync();

        // Assert
        ContentOrderEntity? retrieved = await _context.ContentOrders.FindAsync(order.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(order.Id);
    }

    #endregion

    #region AddItem through the root

    [Fact]
    public async Task AddItem_ThroughTheRoot_ShouldPersistItemToDatabase()
    {
        // Arrange & Act
        (ContentOrderEntity order, ContentOrderItemEntity item) = await SeedOrderWithItemAsync();

        // Assert
        ContentOrderItemEntity? retrieved = await _context.ContentOrderItems.FindAsync(item.Id);
        retrieved.Should().NotBeNull();
        retrieved!.OrderId.Should().Be(order.Id);
    }

    #endregion

    #region AddTier through the root

    [Fact]
    public async Task AddTier_ThroughTheRoot_ShouldPersistTierToDatabase()
    {
        // Arrange
        (ContentOrderEntity order, ContentOrderItemEntity item) = await SeedOrderWithItemAsync();
        PricingTierEntity pricingTier = await SeedPricingTierAsync();

        // Act
        ContentItemTierEntity tier = order.AddTier(item: item, pricingTierId: pricingTier.Id, priceSnapshotUsd: 100m);
        await _context.SaveChangesAsync();

        // Assert
        ContentItemTierEntity? retrieved = await _context.ContentItemTiers.FindAsync(tier.Id);
        retrieved.Should().NotBeNull();
        retrieved!.OrderItemId.Should().Be(item.Id);
    }

    #endregion

    #region AttachPayment through the root

    [Fact]
    public async Task AttachPayment_ThroughTheRoot_ShouldPersistPaymentToDatabase()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();

        // Act
        ContentPaymentEntity payment = order.AttachPayment();
        await _context.SaveChangesAsync();

        // Assert
        ContentPaymentEntity? retrieved = await _context.ContentPayments.FindAsync(payment.Id);
        retrieved.Should().NotBeNull();
        retrieved!.OrderId.Should().Be(order.Id);
    }

    #endregion

    #region Tracked order mutation

    [Fact]
    public async Task TrackedMutation_ShouldUpdateOrderInDatabase()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();

        // Act — a loaded order is tracked; SaveChanges diffs the row without an attach call.
        order.Submit();
        await _context.SaveChangesAsync();

        // Assert
        ContentOrderEntity? updated = await _context.ContentOrders.FindAsync(order.Id);
        updated!.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    #endregion

    #region Tracked payment mutation

    [Fact]
    public async Task TrackedMutation_ShouldUpdatePaymentInDatabase()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        order.Submit();
        ContentPaymentEntity payment = order.AttachPayment();
        payment.AttachProof(Guid.NewGuid(), EnumPaymentMethod.BankTransfer);
        await _context.SaveChangesAsync();

        // Act — tracked mutation
        payment.Verify(
            adminUserId: Guid.NewGuid(),
            receiptUrl: "https://receipts.example.com/test.pdf",
            now: TestConstants.Clock.Instant
        );
        await _context.SaveChangesAsync();

        // Assert
        ContentPaymentEntity? updated = await _context.ContentPayments.FindAsync(payment.Id);
        updated!.Status.Should().Be(EnumPaymentStatus.Verified);
    }

    #endregion

    #region GetByIdWithItemsAsync

    [Fact]
    public async Task GetByIdWithItemsAsync_WhenFound_ShouldReturnOrderWithNavigations()
    {
        // Arrange
        (ContentOrderEntity order, _) = await SeedOrderWithItemAsync();
        _context.ChangeTracker.Clear();

        // Act
        ContentOrderEntity? result = await _repository.GetByIdWithItemsAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        ContentOrderEntity? result = await _repository.GetByIdWithItemsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenFound_ShouldReturnOrder()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();

        // Act
        ContentOrderEntity result = await _repository.GetByIdOrThrowAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedOrders()
    {
        // Arrange
        await SeedOrderAsync();
        await SeedOrderAsync();
        await SeedOrderAsync();

        // Act
        (IReadOnlyList<ContentOrderEntity> items, int total) = await _repository.GetAllAsync(1, 10, null, null);

        // Assert
        total.Should().Be(3);
        items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        ContentOrderEntity draftOrder = await SeedOrderAsync();
        ContentOrderEntity submittedOrder = await SeedOrderAsync();
        submittedOrder.Submit();
        await _context.SaveChangesAsync();

        // Act
        (IReadOnlyList<ContentOrderEntity> items, int total) = await _repository.GetAllAsync(
            1,
            10,
            EnumOrderStatus.Draft,
            null
        );

        // Assert
        total.Should().Be(1);
        items.Should().OnlyContain(o => o.Status == EnumOrderStatus.Draft);
    }

    [Fact]
    public async Task GetAllAsync_WithCustomerIdFilter_ShouldFilterByCustomer()
    {
        // Arrange
        CustomerEntity customer = await SeedCustomerAsync();
        await SeedOrderAsync(customer.Id);
        await SeedOrderAsync(); // different customer

        // Act
        (IReadOnlyList<ContentOrderEntity> items, int total) = await _repository.GetAllAsync(1, 10, null, customer.Id);

        // Assert
        total.Should().Be(1);
        items.Should().OnlyContain(o => o.CustomerId == customer.Id);
    }

    [Fact]
    public async Task GetAllAsync_OrderByAscending_ShouldOrderByCreatedAtAsc()
    {
        // Arrange
        await SeedOrderAsync();
        await SeedOrderAsync();

        // Act
        (IReadOnlyList<ContentOrderEntity> items, _) = await _repository.GetAllAsync(
            1,
            10,
            null,
            null,
            orderByAscending: true
        );

        // Assert
        items.Should().BeInAscendingOrder(o => o.CreatedAt);
    }

    #endregion

    #region Payment hydration

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenPaymentAttached_ShouldHydratePayment()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        order.Submit();
        order.AttachPayment();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        ContentOrderEntity loaded = await _repository.GetByIdOrThrowAsync(order.Id);

        // Assert
        loaded.Payment.Should().NotBeNull();
        loaded.Payment!.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenDraftOrder_ShouldHaveNoPayment()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        _context.ChangeTracker.Clear();

        // Act
        ContentOrderEntity loaded = await _repository.GetByIdOrThrowAsync(order.Id);

        // Assert
        loaded.Payment.Should().BeNull();
    }

    #endregion

    #region FindItem on a rehydrated order

    [Fact]
    public async Task FindItem_OnRehydratedOrder_ShouldReturnItemOrNull()
    {
        // Arrange
        (ContentOrderEntity order, ContentOrderItemEntity item) = await SeedOrderWithItemAsync();
        _context.ChangeTracker.Clear();

        // Act
        ContentOrderEntity loaded = await _repository.GetByIdOrThrowAsync(order.Id);

        // Assert
        loaded.FindItem(item.Id).Should().NotBeNull();
        loaded.FindItem(Guid.NewGuid()).Should().BeNull();
    }

    [Fact]
    public async Task FindItem_OnRehydratedOrder_ShouldHydrateItemTiers()
    {
        // Arrange
        (ContentOrderEntity order, ContentOrderItemEntity item) = await SeedOrderWithItemAsync();
        PricingTierEntity pricingTier = await SeedPricingTierAsync();
        order.AddTier(item: item, pricingTierId: pricingTier.Id, priceSnapshotUsd: 100m);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        ContentOrderEntity loaded = await _repository.GetByIdOrThrowAsync(order.Id);

        // Assert
        loaded.FindItem(item.Id)!.Tiers.Should().ContainSingle();
    }

    #endregion

    #region Tracked item mutation

    [Fact]
    public async Task TrackedMutation_ShouldUpdateItemInDatabase()
    {
        // Arrange
        (_, ContentOrderItemEntity item) = await SeedOrderWithItemAsync();

        Guid newCategoryId = Guid.NewGuid();
        item.Update(
            contentKind: null,
            categoryId: newCategoryId,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: null,
            isBonus: null
        );

        // Act — tracked mutation
        await _context.SaveChangesAsync();

        // Assert
        ContentOrderItemEntity? updated = await _context.ContentOrderItems.FindAsync(item.Id);
        updated!.CategoryId.Should().Be(newCategoryId);
    }

    #endregion

    #region RemoveItem through the root

    [Fact]
    public async Task RemoveItem_ThroughTheRoot_ShouldDeleteItemFromDatabase()
    {
        // Arrange
        (ContentOrderEntity order, ContentOrderItemEntity item) = await SeedOrderWithItemAsync();

        // Act
        order.RemoveItem(item);
        await _context.SaveChangesAsync();

        // Assert
        ContentOrderItemEntity? retrieved = await _context.ContentOrderItems.FindAsync(item.Id);
        retrieved.Should().BeNull();
    }

    #endregion

    #region RemoveTier through the root

    [Fact]
    public async Task RemoveTier_ThroughTheRoot_ShouldDeleteTierFromDatabase()
    {
        // Arrange
        (ContentOrderEntity order, ContentOrderItemEntity item) = await SeedOrderWithItemAsync();
        PricingTierEntity pricingTier = await SeedPricingTierAsync();
        ContentItemTierEntity tier = order.AddTier(item: item, pricingTierId: pricingTier.Id, priceSnapshotUsd: 100m);
        await _context.SaveChangesAsync();

        // Act
        order.RemoveTier(item, tier.Id).Should().BeTrue();
        await _context.SaveChangesAsync();

        // Assert
        ContentItemTierEntity? retrieved = await _context.ContentItemTiers.FindAsync(tier.Id);
        retrieved.Should().BeNull();
    }

    #endregion
}
