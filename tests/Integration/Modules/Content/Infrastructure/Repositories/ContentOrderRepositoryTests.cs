using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IContentOrderRepository"/> verifying order, item, tier,
/// and payment persistence operations against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class ContentOrderRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task AddAsync_PersistsOrderToDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        var (repo, db) = CreateScopedRepository<IContentOrderRepository, ContentDbContext>();

        await repo.AddAsync(order);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.ContentOrders.FindAsync(order.Id);

        persisted.Should().NotBeNull();
        persisted!.CustomerId.Should().Be(customer.Id);
        persisted.Status.Should().Be(EnumOrderStatus.Draft);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenOrderExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentOrderRepository>();

        var result = await repo.GetByIdOrThrowAsync(order.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repo = Resolve<IContentOrderRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_WhenNotFound_ReturnsNull()
    {
        var repo = Resolve<IContentOrderRepository>();

        var result = await repo.GetByIdWithItemsAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyWhenNoOrdersExist()
    {
        var repo = Resolve<IContentOrderRepository>();

        var (items, totalCount) = await repo.GetAllAsync(page: 1, pageSize: 10, status: null, customerId: null);

        totalCount.Should().Be(0);
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var orders = Enumerable.Range(0, 5).Select(_ => ContentOrderFactory.CreateForCustomer(customer.Id)).ToList();
        seedContext.ContentOrders.AddRange(orders);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentOrderRepository>();

        var (result, totalCount) = await repo.GetAllAsync(page: 1, pageSize: 3, status: null, customerId: null);

        totalCount.Should().Be(5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithCustomerIdFilter_ReturnsOnlyMatchingOrders()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customerA = CustomerFactory.Create();
        var customerB = CustomerFactory.Create();
        seedContext.Customers.AddRange(customerA, customerB);
        await seedContext.SaveChangesAsync();

        var orderA = ContentOrderFactory.CreateForCustomer(customerA.Id);
        var orderB = ContentOrderFactory.CreateForCustomer(customerB.Id);
        seedContext.ContentOrders.AddRange(orderA, orderB);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentOrderRepository>();

        var (result, totalCount) = await repo.GetAllAsync(
            page: 1,
            pageSize: 50,
            status: null,
            customerId: customerA.Id
        );

        totalCount.Should().Be(1);
        result.Should().OnlyContain(o => o.CustomerId == customerA.Id);
    }

    [Fact]
    public async Task AddPaymentAsync_PersistsPaymentToDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var payment = ContentPaymentFactory.Create(order.Id, 250.00m);
        var (repo, db) = CreateScopedRepository<IContentOrderRepository, ContentDbContext>();

        await repo.AddPaymentAsync(payment);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.ContentPayments.FindAsync(payment.Id);

        persisted.Should().NotBeNull();
        persisted!.OrderId.Should().Be(order.Id);
        persisted.AmountUsd.Should().Be(250.00m);
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_WhenExists_ReturnsPayment()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var payment = ContentPaymentFactory.Create(order.Id);
        seedContext.ContentPayments.Add(payment);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentOrderRepository>();

        var result = await repo.GetPaymentByOrderIdAsync(order.Id);

        result.Should().NotBeNull();
        result!.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = Resolve<IContentOrderRepository>();

        var result = await repo.GetPaymentByOrderIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
