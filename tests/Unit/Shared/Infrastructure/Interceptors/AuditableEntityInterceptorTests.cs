using _116.Shared.Domain;
using _116.Shared.Infrastructure.interceptors;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Interceptors;

/// <summary>
/// Unit tests for <see cref="AuditableEntityInterceptor"/>.
/// </summary>
public class AuditableEntityInterceptorTests
{
    private class TestEntity : Entity<Guid>, IEntity
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
            .AddInterceptors(new AuditableEntityInterceptor())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public void SavingChanges_WithNewEntity_ShouldSetCreatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        DateTime beforeSave = DateTime.UtcNow;

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        DateTime afterSave = DateTime.UtcNow;
        entity.CreatedBy.Should().Be("system");
        entity.CreatedAt.Should().NotBeNull();
        entity.CreatedAt!.Value.Should().BeOnOrAfter(beforeSave);
        entity.CreatedAt!.Value.Should().BeOnOrBefore(afterSave);
    }

    [Fact]
    public void SavingChanges_WithNewEntity_ShouldSetUpdatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        DateTime beforeSave = DateTime.UtcNow;

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        DateTime afterSave = DateTime.UtcNow;
        entity.UpdatedBy.Should().Be("system");
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt!.Value.Should().BeOnOrAfter(beforeSave);
        entity.UpdatedAt!.Value.Should().BeOnOrBefore(afterSave);
    }

    [Fact]
    public void SavingChanges_WithModifiedEntity_ShouldUpdateUpdatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        DateTime originalUpdatedAt = entity.UpdatedAt!.Value;
        Thread.Sleep(10); // Ensure time difference

        // Act
        entity.Name = "Modified";
        DateTime beforeUpdate = DateTime.UtcNow;
        context.SaveChanges();

        // Assert
        DateTime afterUpdate = DateTime.UtcNow;
        entity.UpdatedBy.Should().Be("system");
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        entity.UpdatedAt!.Value.Should().BeOnOrAfter(beforeUpdate);
        entity.UpdatedAt!.Value.Should().BeOnOrBefore(afterUpdate);
    }

    [Fact]
    public void SavingChanges_WithModifiedEntity_ShouldNotChangeCreatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        string? originalCreatedBy = entity.CreatedBy;
        DateTime? originalCreatedAt = entity.CreatedAt;

        // Act
        entity.Name = "Modified";
        context.SaveChanges();

        // Assert
        entity.CreatedBy.Should().Be(originalCreatedBy);
        entity.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public async Task SavingChangesAsync_WithNewEntity_ShouldSetCreatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        DateTime beforeSave = DateTime.UtcNow;

        // Act
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        DateTime afterSave = DateTime.UtcNow;
        entity.CreatedBy.Should().Be("system");
        entity.CreatedAt.Should().NotBeNull();
        entity.CreatedAt!.Value.Should().BeOnOrAfter(beforeSave);
        entity.CreatedAt!.Value.Should().BeOnOrBefore(afterSave);
    }

    [Fact]
    public async Task SavingChangesAsync_WithModifiedEntity_ShouldUpdateUpdatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        DateTime originalUpdatedAt = entity.UpdatedAt!.Value;
        await Task.Delay(10); // Ensure time difference

        // Act
        entity.Name = "Modified";
        DateTime beforeUpdate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Assert
        DateTime afterUpdate = DateTime.UtcNow;
        entity.UpdatedBy.Should().Be("system");
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        entity.UpdatedAt!.Value.Should().BeOnOrAfter(beforeUpdate);
        entity.UpdatedAt!.Value.Should().BeOnOrBefore(afterUpdate);
    }

    [Fact]
    public void SavingChanges_WithUnchangedEntity_ShouldNotUpdateFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        DateTime? originalUpdatedAt = entity.UpdatedAt;
        string? originalUpdatedBy = entity.UpdatedBy;

        // Act - Save again without modifications
        context.Entry(entity).State = EntityState.Unchanged;
        context.SaveChanges();

        // Assert - Fields should remain unchanged
        entity.UpdatedAt.Should().Be(originalUpdatedAt);
        entity.UpdatedBy.Should().Be(originalUpdatedBy);
    }

    [Fact]
    public void SavingChanges_WithMultipleEntities_ShouldUpdateAll()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 1" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 2" };
        DateTime beforeSave = DateTime.UtcNow;

        // Act
        context.TestEntities.AddRange(entity1, entity2);
        context.SaveChanges();

        // Assert
        DateTime afterSave = DateTime.UtcNow;
        entity1.CreatedBy.Should().Be("system");
        entity1.CreatedAt.Should().BeOnOrAfter(beforeSave).And.BeOnOrBefore(afterSave);
        entity2.CreatedBy.Should().Be("system");
        entity2.CreatedAt.Should().BeOnOrAfter(beforeSave).And.BeOnOrBefore(afterSave);
    }

    [Fact]
    public void SavingChanges_WithDeletedEntity_ShouldNotUpdateFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        DateTime originalUpdatedAt = entity.UpdatedAt!.Value;

        // Act
        context.TestEntities.Remove(entity);
        context.SaveChanges();

        // Assert - Updated fields should not change on delete
        entity.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void SavingChanges_WithUnchangedEntityButModifiedOwnedEntity_ShouldUpdateFields()
    {
        // Arrange - Create entity with owned entity
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            OwnedData = new OwnedEntity { Value = "Original" },
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        DateTime? originalUpdatedAt = entity.UpdatedAt;
        Thread.Sleep(10); // Ensure time difference

        // Modify only the owned entity
        if (entity.OwnedData != null)
        {
            entity.OwnedData.Value = "Modified";
            context.Entry(entity.OwnedData).State = EntityState.Modified;
        }

        // Ensure parent entity is Unchanged
        context.Entry(entity).State = EntityState.Unchanged;
        DateTime beforeSave = DateTime.UtcNow;

        // Act
        context.SaveChanges();

        // Assert - UpdatedBy and UpdatedAt should be set even though parent is Unchanged
        DateTime afterSave = DateTime.UtcNow;
        entity.UpdatedBy.Should().Be("system");
        entity.UpdatedAt.Should().NotBeNull();
        if (originalUpdatedAt.HasValue)
        {
            entity.UpdatedAt.Should().BeAfter(originalUpdatedAt.Value);
        }
        entity.UpdatedAt!.Value.Should().BeOnOrAfter(beforeSave);
        entity.UpdatedAt!.Value.Should().BeOnOrBefore(afterSave);
    }
}
