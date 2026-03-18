using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
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

    #region AddItemAsync

    [Fact]
    public async Task AddItemAsync_ShouldPersistItemToDatabase()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());

        // Act
        await _repository.AddItemAsync(item);
        await _context.SaveChangesAsync();

        // Assert
        ContentOrderItemEntity? retrieved = await _context.ContentOrderItems.FindAsync(item.Id);
        retrieved.Should().NotBeNull();
        retrieved!.OrderId.Should().Be(order.Id);
    }

    #endregion

    #region AddItemTierAsync

    [Fact]
    public async Task AddItemTierAsync_ShouldPersistTierToDatabase()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        await _repository.AddItemAsync(item);
        await _context.SaveChangesAsync();

        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());

        // Act
        await _repository.AddItemTierAsync(tier);
        await _context.SaveChangesAsync();

        // Assert
        ContentItemTierEntity? retrieved = await _context.ContentItemTiers.FindAsync(tier.Id);
        retrieved.Should().NotBeNull();
        retrieved!.OrderItemId.Should().Be(item.Id);
    }

    #endregion

    #region AddPaymentAsync

    [Fact]
    public async Task AddPaymentAsync_ShouldPersistPaymentToDatabase()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        // Act
        await _repository.AddPaymentAsync(payment);
        await _context.SaveChangesAsync();

        // Assert
        ContentPaymentEntity? retrieved = await _context.ContentPayments.FindAsync(payment.Id);
        retrieved.Should().NotBeNull();
        retrieved!.OrderId.Should().Be(order.Id);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOrderInDatabase()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();

        // Act
        order.Submit();
        await _repository.UpdateAsync(order);
        await _context.SaveChangesAsync();

        // Assert
        ContentOrderEntity? updated = await _context.ContentOrders.FindAsync(order.Id);
        updated!.Status.Should().Be(EnumOrderStatus.PendingPayment);
    }

    #endregion

    #region UpdatePaymentAsync

    [Fact]
    public async Task UpdatePaymentAsync_ShouldUpdatePaymentInDatabase()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        order.Submit();
        await _context.SaveChangesAsync();

        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);
        await _repository.AddPaymentAsync(payment);
        await _context.SaveChangesAsync();

        // Act
        payment.Verify(adminUserId: Guid.NewGuid(), receiptUrl: "https://receipts.example.com/test.pdf");
        await _repository.UpdatePaymentAsync(payment);
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
        ContentOrderEntity order = await SeedOrderAsync();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        await _repository.AddItemAsync(item);
        await _context.SaveChangesAsync();

        // Act
        ContentOrderEntity? result = await _repository.GetByIdWithItemsAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Items.Should().HaveCount(1);
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
        await _repository.UpdateAsync(submittedOrder);
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
        // Arrange — seed 2 orders
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

    #region GetPaymentByOrderIdAsync

    [Fact]
    public async Task GetPaymentByOrderIdAsync_WhenFound_ShouldReturnPayment()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);
        await _repository.AddPaymentAsync(payment);
        await _context.SaveChangesAsync();

        // Act
        ContentPaymentEntity? result = await _repository.GetPaymentByOrderIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        ContentPaymentEntity? result = await _repository.GetPaymentByOrderIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetItemByIdAsync

    [Fact]
    public async Task GetItemByIdAsync_WhenFound_ShouldReturnItem()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        await _repository.AddItemAsync(item);
        await _context.SaveChangesAsync();

        // Act
        ContentOrderItemEntity? result = await _repository.GetItemByIdAsync(order.Id, item.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
        result.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetItemByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        ContentOrderEntity order = await SeedOrderAsync();

        // Act
        ContentOrderItemEntity? result = await _repository.GetItemByIdAsync(order.Id, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
