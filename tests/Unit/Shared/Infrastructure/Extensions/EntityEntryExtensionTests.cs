using _116.Shared.Domain;
using _116.Shared.Infrastructure.Extensions;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Extensions;

/// <summary>
/// Unit tests for <see cref="EntityEntryExtension"/>.
/// </summary>
public class EntityEntryExtensionTests
{
    private class TestEntity : Entity<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public OwnedEntity? OwnedData { get; set; }
    }

    [Owned]
    private class OwnedEntity
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().OwnsOne(e => e.OwnedData);
        }
    }

    private TestDbContext CreateInMemoryContext()
    {
        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public void HasChangedOwnedEntities_WithNoOwnedEntities_ShouldReturnFalse()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        context.TestEntities.Add(entity);

        // Act
        EntityEntry<TestEntity> entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChangedOwnedEntities_WithAddedOwnedEntity_ShouldReturnTrue()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            OwnedData = new OwnedEntity { Value = "Owned value" },
        };
        context.TestEntities.Add(entity);

        // Act
        EntityEntry<TestEntity> entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChangedOwnedEntities_WithModifiedOwnedEntity_ShouldReturnTrue()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            OwnedData = new OwnedEntity { Value = "Original" },
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Modify owned entity
        entity.OwnedData.Value = "Modified";
        context.Entry(entity.OwnedData).State = EntityState.Modified;

        // Act
        EntityEntry<TestEntity> entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChangedOwnedEntities_WithUnchangedOwnedEntity_ShouldReturnFalse()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            OwnedData = new OwnedEntity { Value = "Value" },
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Detach and reattach to simulate unchanged state
        context.Entry(entity).State = EntityState.Unchanged;

        // Act
        EntityEntry<TestEntity> entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChangedOwnedEntities_WithDeletedOwnedEntity_ShouldReturnFalse()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            OwnedData = new OwnedEntity { Value = "Value" },
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Mark owned entity as deleted
        if (entity.OwnedData != null)
        {
            context.Entry(entity.OwnedData).State = EntityState.Deleted;
        }

        // Act
        EntityEntry<TestEntity> entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeFalse("deleted state should not count as changed");
    }
}
