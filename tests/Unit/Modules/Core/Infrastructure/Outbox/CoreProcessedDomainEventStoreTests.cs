using _116.Core.Infrastructure.Outbox;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Outbox;

/// <summary>
/// Unit tests for <see cref="CoreProcessedDomainEventStore" />: the Core module's
/// replay guard, which has to claim handler invocations in its own schema.
/// </summary>
public class CoreProcessedDomainEventStoreTests
{
    /// <summary>
    /// Exposes the schema the store builds its claim statement against. The member is protected
    /// on the base, so a derived probe is how a test reads it.
    /// </summary>
    /// <param name="context">The Core module database context.</param>
    private sealed class SchemaProbe(CoreDbContext context) : CoreProcessedDomainEventStore(context)
    {
        public string Schema => SchemaName;
    }

    /// <summary>
    /// Builds a context backed by its own in-memory store.
    /// </summary>
    /// <returns>The context.</returns>
    private static CoreDbContext CreateContext() =>
        new(
            new DbContextOptionsBuilder<CoreDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options
        );

    [Fact]
    public void SchemaName_ShouldTargetTheModulesOwnSchema()
    {
        // Arrange
        // A wrong schema would write the claim into another module's table, and the guard would
        // silently stop suppressing replays.
        using CoreDbContext context = CreateContext();
        var probe = new SchemaProbe(context);

        // Act
        string schema = probe.Schema;

        // Assert
        schema.Should().Be("core");
    }

    [Fact]
    public void Constructor_ShouldProduceTheSharedGuardContract()
    {
        // Arrange
        using CoreDbContext context = CreateContext();

        // Act
        var store = new CoreProcessedDomainEventStore(context);

        // Assert
        store.Should().BeAssignableTo<ProcessedDomainEventStore<CoreDbContext>>();
    }
}
