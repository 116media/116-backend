using _116.Content.Application.Shared.Repositories;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Content.Repositories;

/// <summary>
/// Integration tests for <see cref="ICustomerRepository"/> verifying persistence behavior
/// against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class CustomerRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyWhenNoCustomersExist()
    {
        var repo = Resolve<ICustomerRepository>();

        var (customers, totalCount) = await repo.GetAllAsync(page: 1, pageSize: 10);

        totalCount.Should().Be(0);
        customers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var seeded = CustomerFactory.CreateMany(5);
        seedContext.Customers.AddRange(seeded);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICustomerRepository>();

        var (result, totalCount) = await repo.GetAllAsync(page: 1, pageSize: 3);

        totalCount.Should().Be(5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCorrectSecondPage()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var seeded = CustomerFactory.CreateMany(5);
        seedContext.Customers.AddRange(seeded);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICustomerRepository>();

        var (result, totalCount) = await repo.GetAllAsync(page: 2, pageSize: 3);

        totalCount.Should().Be(5);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICustomerRepository>();

        var result = await repo.GetByIdAsync(customer.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<ICustomerRepository>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenCustomerExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICustomerRepository>();

        var result = await repo.GetByIdOrThrowAsync(customer.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        var repo = Resolve<ICustomerRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenCustomerExists_ReturnsEntity()
    {
        var email = "integration-test@example.com";
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create(email);
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICustomerRepository>();

        var result = await repo.GetByEmailAsync(email);

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<ICustomerRepository>();

        var result = await repo.GetByEmailAsync("nonexistent@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_PersistsCustomerToDatabase()
    {
        var (repo, db) = CreateScopedRepository<ICustomerRepository, ContentDbContext>();
        var customer = CustomerFactory.Create();

        await repo.AddAsync(customer);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);

        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(customer.Id);
    }
}
