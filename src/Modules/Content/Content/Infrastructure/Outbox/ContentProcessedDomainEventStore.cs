using _116.Content.Domain.Constants;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;

namespace _116.Content.Infrastructure.Outbox;

/// <summary>
/// The Content module's processed-event guard, scoped to its own schema.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ContentProcessedDomainEventStore(ContentDbContext context)
    : ProcessedDomainEventStore<ContentDbContext>(context)
{
    /// <inheritdoc />
    protected override string SchemaName => ContentConstants.SchemaName;
}
