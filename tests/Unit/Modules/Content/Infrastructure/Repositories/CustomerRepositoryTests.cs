using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="CustomerRepository"/> using InMemory database.
/// </summary>
public class CustomerRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new CustomerRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldPersistCustomerEntity()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.CreateDefault();

        // Act
        await _repository.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Assert
        CustomerEntity? retrieved = await _context.Customers.FindAsync(customer.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be(customer.Email);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.CreateDefault();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        CustomerEntity? result = await _repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        CustomerEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.CreateDefault();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        CustomerEntity result = await _repository.GetByIdOrThrowAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact(
        Skip = "CustomerByEmailSpecification uses EF.Functions.ILike which is not supported by InMemoryDatabase — tested in integration tests"
    )]
    public async Task GetByEmailAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        string email = TestConstants.Content.Customer.ValidEmail;
        CustomerEntity customer = CustomerFactory.Create(email);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        CustomerEntity? result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        CustomerEntity? result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        _context.Customers.AddRange(CustomerFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        (List<CustomerEntity> customers, int totalCount) = await _repository.GetAllAsync(page: 1, pageSize: 3);

        // Assert
        customers.Should().HaveCount(3);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_SecondPage_ShouldReturnNextPageItems()
    {
        // Arrange
        _context.Customers.AddRange(CustomerFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        (List<CustomerEntity> customers, int totalCount) = await _repository.GetAllAsync(page: 2, pageSize: 3);

        // Assert
        customers.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act
        (List<CustomerEntity> customers, int totalCount) = await _repository.GetAllAsync(page: 1, pageSize: 10);

        // Assert
        customers.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    #endregion
}
