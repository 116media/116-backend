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
/// Unit tests for <see cref="PricingTierRepository"/> using InMemory database.
/// </summary>
public class PricingTierRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly PricingTierRepository _repository;

    public PricingTierRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new PricingTierRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEntity()
    {
        // Arrange
        PricingTierEntity pricingTier = PricingTierFactory.Create("base_upload");

        // Act
        await _repository.AddAsync(pricingTier);
        await _context.SaveChangesAsync();

        // Assert
        PricingTierEntity? retrieved = await _context.PricingTiers.FindAsync(pricingTier.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        // Act
        PricingTierEntity result = await _repository.GetByIdOrThrowAsync(pricingTier.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(pricingTier.Id);
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

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrderedByName()
    {
        // Arrange
        _context.PricingTiers.Add(PricingTierFactory.Create("social_boost"));
        _context.PricingTiers.Add(PricingTierFactory.Create("base_upload"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<PricingTierEntity> result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("base_upload");
        result[1].Name.Should().Be("social_boost");
    }

    // NOTE: PricingTierExistsByNameAsync and PricingTierSearchSpecification use ILike — tested in integration tests.
}
