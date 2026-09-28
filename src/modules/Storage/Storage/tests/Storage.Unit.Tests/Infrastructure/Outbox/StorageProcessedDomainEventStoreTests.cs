using _116.BuildingBlocks.Infrastructure.Outbox;
using _116.Storage.Infrastructure.Outbox;
using _116.Storage.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Storage.Unit.Tests.Infrastructure.Outbox;

/// <summary>
/// Unit tests for <see cref="StorageProcessedDomainEventStore" />: the Storage module's
/// replay guard, which has to claim handler invocations in its own schema.
/// </summary>
public class StorageProcessedDomainEventStoreTests
{
    /// <summary>
    /// Exposes the schema the store builds its claim statement against. The member is protected
    /// on the base, so a derived probe is how a test reads it.
    /// </summary>
    /// <param name="context">The Storage module database context.</param>
    private sealed class SchemaProbe(StorageDbContext context) : StorageProcessedDomainEventStore(context)
    {
        public string Schema => SchemaName;
    }

    /// <summary>
    /// Builds a context backed by its own in-memory store.
    /// </summary>
    /// <returns>The context.</returns>
    private static StorageDbContext CreateContext() =>
        new(
            new DbContextOptionsBuilder<StorageDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options
        );

    [Fact]
    public void SchemaName_ShouldTargetTheModulesOwnSchema()
    {
        // Arrange
        // A wrong schema would write the claim into another module's table, and the guard would
        // silently stop suppressing replays.
        using StorageDbContext context = CreateContext();
        var probe = new SchemaProbe(context);

        // Act
        string schema = probe.Schema;

        // Assert
        schema.Should().Be("storage");
    }

    [Fact]
    public void Constructor_ShouldProduceTheSharedGuardContract()
    {
        // Arrange
        using StorageDbContext context = CreateContext();

        // Act
        var store = new StorageProcessedDomainEventStore(context);

        // Assert
        store.Should().BeAssignableTo<ProcessedDomainEventStore<StorageDbContext>>();
    }
}
