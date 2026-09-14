using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
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
    public async Task GetAllAsync_WithStatusFilter_ReturnsOnlyMatchingOrders()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        var draft = ContentOrderFactory.CreateForCustomer(customer.Id);
        var paid = new ContentOrderBuilder().WithCustomerId(customer.Id).AsPaid().Build();
        seedContext.ContentOrders.AddRange(draft, paid);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentOrderRepository>();

        var (result, _) = await repo.GetAllAsync(page: 1, pageSize: 50, status: EnumOrderStatus.Paid, customerId: null);

        result.Should().OnlyContain(o => o.Status == EnumOrderStatus.Paid);
        result.Should().Contain(o => o.Id == paid.Id);
        result.Should().NotContain(o => o.Id == draft.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_MatchesCustomerNameEmailOrCompany()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        string marker = $"srch{Guid.NewGuid():N}"[..12];
        var byName = new CustomerBuilder().WithFullName($"{marker}-name").Build();
        var byEmail = new CustomerBuilder().WithEmail($"{marker}@example.com").Build();
        var byCompany = new CustomerBuilder().WithCompany($"{marker} Media").Build();
        var unrelated = CustomerFactory.Create();
        seedContext.Customers.AddRange(byName, byEmail, byCompany, unrelated);
        var orders = new[] { byName.Id, byEmail.Id, byCompany.Id, unrelated.Id }
            .Select(ContentOrderFactory.CreateForCustomer)
            .ToArray();
        seedContext.ContentOrders.AddRange(orders);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentOrderRepository>();

        var (result, totalCount) = await repo.GetAllAsync(
            page: 1,
            pageSize: 50,
            status: null,
            customerId: null,
            search: marker.ToUpperInvariant()
        );

        totalCount.Should().Be(3);
        result.Select(o => o.CustomerId).Should().BeEquivalentTo([byName.Id, byEmail.Id, byCompany.Id]);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_WithStatusAndMethodFilters_ReturnsOnlyMatchingPayments()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        var pendingOrder = ContentOrderFactory.CreateForCustomer(customer.Id);
        var verifiedOrder = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.AddRange(pendingOrder, verifiedOrder);
        var pending = ContentPaymentFactory.Create(pendingOrder.Id);
        var verified = ContentPaymentFactory.CreateVerified(verifiedOrder.Id);
        seedContext.ContentPayments.AddRange(pending, verified);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentOrderRepository>();

        var (byStatus, _) = await repo.GetAllPaymentsAsync(
            page: 1,
            pageSize: 50,
            status: EnumPaymentStatus.Verified,
            method: null
        );

        byStatus.Should().OnlyContain(p => p.Status == EnumPaymentStatus.Verified);
        byStatus.Should().Contain(p => p.Id == verified.Id);
        byStatus.Should().NotContain(p => p.Id == pending.Id);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_WithSearch_MatchesTheOrderingCustomer()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        string marker = $"pay{Guid.NewGuid():N}"[..12];
        var wanted = new CustomerBuilder().WithFullName($"{marker}-customer").Build();
        var other = CustomerFactory.Create();
        seedContext.Customers.AddRange(wanted, other);
        var wantedOrder = ContentOrderFactory.CreateForCustomer(wanted.Id);
        var otherOrder = ContentOrderFactory.CreateForCustomer(other.Id);
        seedContext.ContentOrders.AddRange(wantedOrder, otherOrder);
        seedContext.ContentPayments.AddRange(
            ContentPaymentFactory.Create(wantedOrder.Id),
            ContentPaymentFactory.Create(otherOrder.Id)
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentOrderRepository>();

        var (result, totalCount) = await repo.GetAllPaymentsAsync(
            page: 1,
            pageSize: 50,
            status: null,
            method: null,
            search: marker
        );

        totalCount.Should().Be(1);
        result.Should().ContainSingle().Which.OrderId.Should().Be(wantedOrder.Id);
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
