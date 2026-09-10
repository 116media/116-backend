using _116.Core.Domain.Constants;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;

namespace _116.Core.Infrastructure.Outbox;

/// <summary>
/// The Core module's processed-event guard, scoped to its own schema.
/// </summary>
/// <param name="context">The Core module database context.</param>
public class CoreProcessedDomainEventStore(CoreDbContext context) : ProcessedDomainEventStore<CoreDbContext>(context)
{
    /// <inheritdoc />
    protected override string SchemaName => CoreConstants.SchemaName;
}
