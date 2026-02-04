using _116.Shared.Domain;
using _116.Shared.Infrastructure.Extensions;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
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
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public void HasChangedOwnedEntities_WithNoOwnedEntities_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        context.TestEntities.Add(entity);

        // Act
        var entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChangedOwnedEntities_WithAddedOwnedEntity_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            OwnedData = new OwnedEntity { Value = "Owned value" },
        };
        context.TestEntities.Add(entity);

        // Act
        var entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChangedOwnedEntities_WithModifiedOwnedEntity_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
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
        var entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChangedOwnedEntities_WithUnchangedOwnedEntity_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
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
        var entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChangedOwnedEntities_WithDeletedOwnedEntity_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
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
        var entry = context.Entry(entity);
        bool hasChanges = entry.HasChangedOwnedEntities();

        // Assert
        hasChanges.Should().BeFalse("deleted state should not count as changed");
    }
}
