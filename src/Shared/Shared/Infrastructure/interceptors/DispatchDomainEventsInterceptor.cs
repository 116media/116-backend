using System.Runtime.CompilerServices;
using _116.Shared.Application.Services;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace _116.Shared.Infrastructure.interceptors;

/// <summary>
/// Dispatches domain events after a successful save: collected from tracked aggregates during
/// the save, buffered per <see cref="DbContext" />, published once the commit completes so
/// handlers only observe committed state. A failed or canceled save discards the buffer.
/// </summary>
/// <remarks>
/// Saves inside an explicit transaction skip in-process dispatch — the transaction has not
/// committed, so handlers would read state no other connection can see. Their outbox rows are
/// durable and the replay job delivers them.
/// </remarks>
public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private static readonly ConditionalWeakTable<DbContext, List<IDomainEvent>> BufferedEvents = new();

    private static readonly ConditionalWeakTable<DbContext, List<OutboxEventEntity>> BufferedOutboxRows = new();

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly ILogger<DispatchDomainEventsInterceptor> _logger;

    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatchDomainEventsInterceptor"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Factory to create scoped services for domain event publishing.</param>
    /// <param name="logger">Logger recording dispatch failures that must not surface to the caller.</param>
    /// <param name="timeProvider">The clock every event's occurrence stamp is read from.</param>
    public DispatchDomainEventsInterceptor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DispatchDomainEventsInterceptor> logger,
        TimeProvider timeProvider
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CollectDomainEvents(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        CollectDomainEvents(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        DispatchBufferedEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    /// <inheritdoc />
    /// <remarks>
    /// The caller's token is not forwarded: reactions to a committed change outlive the request,
    /// so a client disconnecting must not skip the revocations and counter updates it owes.
    /// </remarks>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        await DispatchBufferedEvents(eventData.Context);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        DiscardBufferedEvents(eventData.Context);
        base.SaveChangesFailed(eventData);
    }

    /// <inheritdoc />
    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        DiscardBufferedEvents(eventData.Context);
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    /// <inheritdoc />
    public override Task SaveChangesCanceledAsync(
        DbContextEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        DiscardBufferedEvents(eventData.Context);
        return base.SaveChangesCanceledAsync(eventData, cancellationToken);
    }

    /// <summary>
    /// Moves pending events off tracked aggregates into the per-context buffer, stamping each.
    /// Clearing here keeps aggregates clean whether the save succeeds or fails, and stops
    /// execution-strategy retries re-buffering.
    /// </summary>
    /// <param name="context">The current DbContext instance.</param>
    private void CollectDomainEvents(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        List<IAggregate> aggregates = context
            .ChangeTracker.Entries<IAggregate>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        if (aggregates.Count == 0)
        {
            return;
        }

        List<IDomainEvent> buffer = BufferedEvents.GetOrCreateValue(context);
        List<OutboxEventEntity> outboxRows = BufferedOutboxRows.GetOrCreateValue(context);

        bool hasOutbox = context.Model.FindEntityType(typeof(OutboxEventEntity)) is not null;

        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (IAggregate aggregate in aggregates)
        {
            foreach (IDomainEvent raised in aggregate.ClearDomainEvents())
            {
                IDomainEvent domainEvent = StampOccurredOn(raised, now);
                buffer.Add(domainEvent);

                if (!hasOutbox)
                {
                    continue;
                }

                OutboxEventEntity row = OutboxEventEntity.Create(domainEvent);
                context.Set<OutboxEventEntity>().Add(row);
                outboxRows.Add(row);
            }
        }
    }

    /// <summary>
    /// Stamps an event's occurrence time, leaving an event that already carries one untouched.
    /// </summary>
    /// <param name="domainEvent">The event as the aggregate raised it.</param>
    /// <param name="now">The current UTC time.</param>
    /// <returns>The event carrying an occurrence stamp.</returns>
    private static IDomainEvent StampOccurredOn(IDomainEvent domainEvent, DateTime now)
    {
        return domainEvent is DomainEvent record && record.OccurredOn == default
            ? record with
            {
                OccurredOn = now,
            }
            : domainEvent;
    }

    /// <summary>
    /// Publishes the context's buffered events in raise order, each in a fresh scope and under
    /// <see cref="CancellationToken.None" />. Every dispatch failure is logged and swallowed —
    /// the change is already committed, so it must not surface as a failed operation.
    /// </summary>
    /// <param name="context">The current DbContext instance.</param>
    /// <returns>A task that completes once all buffered events are published.</returns>
    private async Task DispatchBufferedEvents(DbContext? context)
    {
        if (context == null || !BufferedEvents.TryGetValue(context, out List<IDomainEvent>? domainEvents))
        {
            return;
        }

        if (context.Database.CurrentTransaction is not null)
        {
            BufferedEvents.Remove(context);
            BufferedOutboxRows.Remove(context);
            return;
        }

        BufferedOutboxRows.TryGetValue(context, out List<OutboxEventEntity>? outboxRows);
        BufferedEvents.Remove(context);
        BufferedOutboxRows.Remove(context);

        var rowsByEventId = outboxRows?.ToDictionary(row => row.Id) ?? [];
        var outcomeRecorded = false;

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            OutboxEventEntity? row = rowsByEventId.GetValueOrDefault(domainEvent.EventId);

            try
            {
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                var domainEventPublisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();

                await domainEventPublisher.Publish(domainEvent, CancellationToken.None);

                row?.MarkDispatched();
                outcomeRecorded |= row is not null;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Dispatching domain event {EventType} failed after the save committed. Event: {DomainEvent}",
                    domainEvent.GetType().Name,
                    domainEvent
                );

                row?.MarkFailed(exception.Message);
                outcomeRecorded |= row is not null;
            }
        }

        if (outcomeRecorded)
        {
            await PersistDispatchOutcomesAsync(context);
        }
    }

    /// <summary>
    /// Writes dispatch outcomes back to the outbox rows. Failures are swallowed and leave the
    /// rows undispatched for the replay job.
    /// </summary>
    /// <param name="context">The context whose outbox rows carry the outcomes.</param>
    private async Task PersistDispatchOutcomesAsync(DbContext context)
    {
        try
        {
            await context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Recording domain event dispatch outcomes failed; replay will retry them.");
        }
    }

    /// <summary>
    /// Drops the context's buffered events so a failed or canceled save dispatches nothing and
    /// leaves none behind for a later save.
    /// </summary>
    /// <param name="context">The current DbContext instance.</param>
    private static void DiscardBufferedEvents(DbContext? context)
    {
        if (context != null)
        {
            BufferedEvents.Remove(context);
            BufferedOutboxRows.Remove(context);
        }
    }
}
