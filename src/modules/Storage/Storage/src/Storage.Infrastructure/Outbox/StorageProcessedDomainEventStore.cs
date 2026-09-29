using _116.BuildingBlocks.Infrastructure.Outbox;
using _116.Storage.Domain.Constants;
using _116.Storage.Infrastructure.Persistence;

namespace _116.Storage.Infrastructure.Outbox;

/// <summary>
/// The Storage module's processed-event guard, scoped to its own schema.
/// </summary>
/// <param name="context">The Storage module database context.</param>
public class StorageProcessedDomainEventStore(StorageDbContext context)
    : ProcessedDomainEventStore<StorageDbContext>(context)
{
    /// <inheritdoc />
    protected override string SchemaName => StorageConstants.SchemaName;
}
