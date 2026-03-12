using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="PackageRepository"/> using InMemory database.
/// </summary>
public class PackageRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly PackageRepository _repository;

    public PackageRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new PackageRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldPersistPackageEntity()
    {
        // Arrange
        PackageEntity package = PackageFactory.CreateDefault();

        // Act
        await _repository.AddAsync(package);
        await _context.SaveChangesAsync();

        // Assert
        PackageEntity? retrieved = await _context.Packages.FindAsync(package.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(package.Name);
    }

    #endregion

    #region GetByIdWithSlotsAsync Tests

    [Fact]
    public async Task GetByIdWithSlotsAsync_WhenFound_ShouldReturnEntityWithSlots()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = PackageSlotFactory.Create(package.Id);

        _context.Packages.Add(package);
        _context.PackageSlots.Add(slot);
        await _context.SaveChangesAsync();

        // Act
        PackageEntity? result = await _repository.GetByIdWithSlotsAsync(package.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(package.Id);
        result.Slots.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdWithSlotsAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        PackageEntity? result = await _repository.GetByIdWithSlotsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithSlotsOrThrowAsync Tests

    [Fact]
    public async Task GetByIdWithSlotsOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        // Act
        PackageEntity result = await _repository.GetByIdWithSlotsOrThrowAsync(package.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(package.Id);
    }

    [Fact]
    public async Task GetByIdWithSlotsOrThrowAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdWithSlotsOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        _context.Packages.AddRange(PackageFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        (List<PackageEntity> packages, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 3,
            isActive: null
        );

        // Assert
        packages.Should().HaveCount(3);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WithIsActiveFilter_ShouldReturnFilteredPackages()
    {
        // Arrange
        _context.Packages.Add(PackageFactory.Create());
        _context.Packages.Add(PackageFactory.Create());
        _context.Packages.Add(PackageFactory.CreateInactive());
        await _context.SaveChangesAsync();

        // Act
        (List<PackageEntity> active, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            isActive: true
        );

        // Assert
        active.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_SecondPage_ShouldReturnNextPageItems()
    {
        // Arrange
        _context.Packages.AddRange(PackageFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        (List<PackageEntity> packages, int totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 3,
            isActive: null
        );

        // Assert
        packages.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    #endregion

    #region Slot Tests

    [Fact]
    public async Task AddSlotAsync_ShouldPersistSlotEntity()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        PackageSlotEntity slot = PackageSlotFactory.Create(package.Id);

        // Act
        await _repository.AddSlotAsync(slot);
        await _context.SaveChangesAsync();

        // Assert
        PackageSlotEntity? retrieved = await _context.PackageSlots.FindAsync(slot.Id);
        retrieved.Should().NotBeNull();
        retrieved!.PackageId.Should().Be(package.Id);
    }

    [Fact]
    public async Task GetSlotByIdAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = PackageSlotFactory.Create(package.Id);

        _context.Packages.Add(package);
        _context.PackageSlots.Add(slot);
        await _context.SaveChangesAsync();

        // Act
        PackageSlotEntity? result = await _repository.GetSlotByIdAsync(slot.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(slot.Id);
    }

    [Fact]
    public async Task GetSlotByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        PackageSlotEntity? result = await _repository.GetSlotByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveSlot_ShouldDeleteSlotFromDatabase()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = PackageSlotFactory.Create(package.Id);

        _context.Packages.Add(package);
        _context.PackageSlots.Add(slot);
        await _context.SaveChangesAsync();

        // Act
        _repository.RemoveSlot(slot);
        await _context.SaveChangesAsync();

        // Assert
        PackageSlotEntity? retrieved = await _context.PackageSlots.FindAsync(slot.Id);
        retrieved.Should().BeNull();
    }

    #endregion
}
