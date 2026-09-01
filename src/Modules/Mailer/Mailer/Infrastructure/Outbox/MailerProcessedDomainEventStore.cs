using _116.Mailer.Domain.Constants;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;

namespace _116.Mailer.Infrastructure.Outbox;

/// <summary>
/// The Mailer module's processed-event guard, scoped to its own schema.
/// </summary>
/// <param name="context">The Mailer module database context.</param>
public class MailerProcessedDomainEventStore(MailerDbContext context)
    : ProcessedDomainEventStore<MailerDbContext>(context)
{
    /// <inheritdoc />
    protected override string SchemaName => MailerConstants.SchemaName;
}
