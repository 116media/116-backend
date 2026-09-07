using _116.Shared.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace _116.Shared.Infrastructure.Outbox;

/// <summary>
/// Claims handler invocations through an insert that loses the race silently, so two dispatchers
/// replaying the same event cannot both run a non-idempotent handler.
/// </summary>
/// <typeparam name="TContext">The module context owning the processed-events table.</typeparam>
/// <param name="context">The module database context.</param>
public abstract class ProcessedDomainEventStore<TContext>(TContext context) : IProcessedDomainEventStore
    where TContext : DbContext
{
    /// <summary>
    /// The schema the module's processed-events table lives in.
    /// </summary>
    protected abstract string SchemaName { get; }

    /// <inheritdoc />
    public async Task<bool> TryRecordAsync(
        Guid eventId,
        string handlerName,
        CancellationToken cancellationToken = default
    )
    {
        string sql =
            $"INSERT INTO {SchemaName}.processed_domain_events (event_id, handler_name, processed_at) "
            + "VALUES ({0}, {1}, {2}) ON CONFLICT (event_id, handler_name) DO NOTHING";

        int inserted = await context.Database.ExecuteSqlRawAsync(
            sql,
            [eventId, handlerName, DateTime.UtcNow],
            cancellationToken
        );

        return inserted > 0;
    }
}
