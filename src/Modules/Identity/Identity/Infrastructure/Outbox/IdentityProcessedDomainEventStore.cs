using _116.Identity.Domain.Constants;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;

namespace _116.Identity.Infrastructure.Outbox;

/// <summary>
/// The Identity module's processed-event guard, scoped to its own schema.
/// </summary>
/// <param name="context">The Identity module database context.</param>
public class IdentityProcessedDomainEventStore(IdentityDbContext context)
    : ProcessedDomainEventStore<IdentityDbContext>(context)
{
    /// <inheritdoc />
    protected override string SchemaName => IdentityConstants.SchemaName;
}
