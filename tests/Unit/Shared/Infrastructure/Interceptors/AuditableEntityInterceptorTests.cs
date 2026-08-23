using _116.Shared.Application.Services;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.interceptors;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;
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

    /// <summary>
    /// The instant the interceptor's clock starts at. Every timestamp assertion below is a
    /// literal offset from it, so nothing is derived from the clock the subject reads.
    /// </summary>
    private static readonly DateTime StartInstant = new(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The clock the interceptor stamps from. xUnit builds a new class instance per fact,
    /// so advancement never leaks between tests.
    /// </summary>
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(StartInstant));

    private TestDbContext CreateInMemoryContext(ICurrentActor? actor = null)
    {
        ICurrentActor currentActor =
            actor
            ?? Mock.Of<ICurrentActor>(a => a.UserId == null && a.IsAuthenticated == false && a.HasHttpContext == false);

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new AuditableEntityInterceptor(currentActor, _time))
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public void SavingChanges_WithNewEntity_ShouldSetCreatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        entity.CreatedBy.Should().Be("System");
        entity.CreatedAt.Should().Be(StartInstant);
    }

    [Fact]
    public void SavingChanges_WithNewEntity_ShouldSetUpdatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        entity.UpdatedBy.Should().Be("System");
        entity.UpdatedAt.Should().Be(StartInstant);
    }

    [Fact]
    public void SavingChanges_WithModifiedEntity_ShouldUpdateUpdatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        entity.UpdatedAt.Should().Be(StartInstant);
        _time.Advance(TimeSpan.FromMinutes(1));

        // Act
        entity.Name = "Modified";
        context.SaveChanges();

        // Assert
        entity.UpdatedBy.Should().Be("System");
        entity.UpdatedAt.Should().Be(StartInstant.AddMinutes(1));
    }

    [Fact]
    public void SavingChanges_WithModifiedEntity_ShouldNotChangeCreatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        entity.CreatedAt.Should().Be(StartInstant);
        _time.Advance(TimeSpan.FromMinutes(1));

        // Act
        entity.Name = "Modified";
        context.SaveChanges();

        // Assert
        entity.CreatedBy.Should().Be("System");
        entity.CreatedAt.Should().Be(StartInstant);
    }

    [Fact]
    public async Task SavingChangesAsync_WithNewEntity_ShouldSetCreatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedBy.Should().Be("System");
        entity.CreatedAt.Should().Be(StartInstant);
    }

    [Fact]
    public async Task SavingChangesAsync_WithModifiedEntity_ShouldUpdateUpdatedFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.UpdatedAt.Should().Be(StartInstant);
        _time.Advance(TimeSpan.FromMinutes(1));

        // Act
        entity.Name = "Modified";
        await context.SaveChangesAsync();

        // Assert
        entity.UpdatedBy.Should().Be("System");
        entity.UpdatedAt.Should().Be(StartInstant.AddMinutes(1));
    }

    [Fact]
    public void SavingChanges_WithUnchangedEntity_ShouldNotUpdateFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        entity.UpdatedAt.Should().Be(StartInstant);
        _time.Advance(TimeSpan.FromMinutes(1));

        // Act
        context.Entry(entity).State = EntityState.Unchanged;
        context.SaveChanges();

        // Assert
        entity.UpdatedAt.Should().Be(StartInstant);
        entity.UpdatedBy.Should().Be("System");
    }

    [Fact]
    public void SavingChanges_WithMultipleEntities_ShouldUpdateAll()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 1" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 2" };

        // Act
        context.TestEntities.AddRange(entity1, entity2);
        context.SaveChanges();

        // Assert
        entity1.CreatedBy.Should().Be("System");
        entity1.CreatedAt.Should().Be(StartInstant);
        entity2.CreatedBy.Should().Be("System");
        entity2.CreatedAt.Should().Be(StartInstant);
    }

    [Fact]
    public void SavingChanges_WithDeletedEntity_ShouldNotUpdateFields()
    {
        // Arrange
        using TestDbContext context = CreateInMemoryContext();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        entity.UpdatedAt.Should().Be(StartInstant);
        _time.Advance(TimeSpan.FromMinutes(1));

        // Act
        context.TestEntities.Remove(entity);
        context.SaveChanges();

        // Assert
        entity.UpdatedAt.Should().Be(StartInstant);
    }

    [Fact]
    public void SavingChanges_WithUnchangedEntityButModifiedOwnedEntity_ShouldUpdateFields()
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

        entity.UpdatedAt.Should().Be(StartInstant);
        _time.Advance(TimeSpan.FromMinutes(1));

        if (entity.OwnedData != null)
        {
            entity.OwnedData.Value = "Modified";
            context.Entry(entity.OwnedData).State = EntityState.Modified;
        }

        context.Entry(entity).State = EntityState.Unchanged;

        // Act
        context.SaveChanges();

        // Assert
        entity.UpdatedBy.Should().Be("System");
        entity.UpdatedAt.Should().Be(StartInstant.AddMinutes(1));
    }

    #region ResolveActor

    [Fact]
    public void SavingChanges_WhenAuthenticated_ShouldSetUserIdAsActor()
    {
        // Arrange
        var actor = Mock.Of<ICurrentActor>(a =>
            a.UserId == "user-abc" && a.IsAuthenticated == true && a.HasHttpContext == true
        );
        using TestDbContext context = CreateInMemoryContext(actor);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        entity.CreatedBy.Should().Be("user-abc");
        entity.UpdatedBy.Should().Be("user-abc");
    }

    [Fact]
    public void SavingChanges_WhenAnonymousRequest_ShouldSetAnonymousAsActor()
    {
        // Arrange
        var actor = Mock.Of<ICurrentActor>(a =>
            a.UserId == null && a.IsAuthenticated == false && a.HasHttpContext == true
        );
        using TestDbContext context = CreateInMemoryContext(actor);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        entity.CreatedBy.Should().Be(nameof(EnumAuditActor.Anonymous));
        entity.UpdatedBy.Should().Be(nameof(EnumAuditActor.Anonymous));
    }

    [Fact]
    public void SavingChanges_WhenNoHttpContext_ShouldSetSystemAsActor()
    {
        // Arrange
        var actor = Mock.Of<ICurrentActor>(a =>
            a.UserId == null && a.IsAuthenticated == false && a.HasHttpContext == false
        );
        using TestDbContext context = CreateInMemoryContext(actor);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        entity.CreatedBy.Should().Be(nameof(EnumAuditActor.System));
        entity.UpdatedBy.Should().Be(nameof(EnumAuditActor.System));
    }

    [Fact]
    public async Task SavingChangesAsync_WhenAuthenticated_ShouldSetUserIdAsActor()
    {
        // Arrange
        var actor = Mock.Of<ICurrentActor>(a =>
            a.UserId == "user-xyz" && a.IsAuthenticated == true && a.HasHttpContext == true
        );
        using TestDbContext context = CreateInMemoryContext(actor);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedBy.Should().Be("user-xyz");
        entity.UpdatedBy.Should().Be("user-xyz");
    }

    #endregion
}
