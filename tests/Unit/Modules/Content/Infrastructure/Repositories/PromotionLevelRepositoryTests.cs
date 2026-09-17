using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="PromotionLevelRepository"/> using InMemory database.
/// </summary>
public class PromotionLevelRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly PromotionLevelRepository _repository;

    public PromotionLevelRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new CreatedAtStampingInterceptor())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new PromotionLevelRepository(_context);
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
        PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create();

        // Act
        await _repository.AddAsync(promotionLevel);
        await _context.SaveChangesAsync();

        // Assert
        PromotionLevelEntity? retrieved = await _context.PromotionLevels.FindAsync(promotionLevel.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        PromotionLevelEntity promotionLevel = PromotionLevelFactory.CreateDefault();
        _context.PromotionLevels.Add(promotionLevel);
        await _context.SaveChangesAsync();

        // Act
        PromotionLevelEntity result = await _repository.GetByIdOrThrowAsync(promotionLevel.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(promotionLevel.Id);
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
    public async Task GetAllAsync_ShouldReturnAll()
    {
        // Arrange
        _context.PromotionLevels.Add(PromotionLevelFactory.Create());
        _context.PromotionLevels.Add(PromotionLevelFactory.Create());
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<PromotionLevelEntity> result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActiveEntities()
    {
        // Arrange
        _context.PromotionLevels.Add(PromotionLevelFactory.CreateDefault());
        _context.PromotionLevels.Add(PromotionLevelFactory.CreateInactive());
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<PromotionLevelEntity> result = await _repository.GetActiveAsync();

        // Assert
        result.Should().ContainSingle();
        result[0].IsActive.Should().BeTrue();
    }

    // NOTE: PromotionLevelExistsByNameAsync and PromotionLevelSearchSpecification use ILike — tested in integration tests.
}
