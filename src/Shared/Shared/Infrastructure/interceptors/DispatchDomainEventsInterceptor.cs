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
/// EF Core interceptor that dispatches domain events after a successful save.
/// Events are collected from tracked aggregates while changes are being saved,
/// buffered per <see cref="DbContext"/>, and published once the commit has
/// completed, so handlers only ever observe committed state and a handler
/// failure can never fail the business operation. When the save fails or is
/// canceled the buffered events are discarded and nothing dispatches.
/// </summary>
/// <remarks>
/// A save inside an explicit transaction completes before that transaction commits, so
/// publishing there would let handlers observe state no other connection can read yet, and a
/// later rollback would leave the reactions behind. Such saves therefore skip in-process
/// dispatch: their outbox rows are already durable, and the replay job delivers them once the
/// transaction has committed.
/// </remarks>
public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private static readonly ConditionalWeakTable<DbContext, List<IDomainEvent>> BufferedEvents = new();

    private static readonly ConditionalWeakTable<DbContext, List<OutboxEventEntity>> BufferedOutboxRows = new();

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly ILogger<DispatchDomainEventsInterceptor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatchDomainEventsInterceptor"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Factory to create scoped services for domain event publishing.</param>
    /// <param name="logger">Logger recording dispatch failures that must not surface to the caller.</param>
    public DispatchDomainEventsInterceptor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DispatchDomainEventsInterceptor> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
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
    /// The caller's cancellation token is deliberately not forwarded to the dispatch. Endpoints
    /// bind it to the request lifetime, and the reactions to a committed change outlive the
    /// request that triggered them: a client disconnecting after the commit must not skip the
    /// session revocations, outbox rows, and counter updates the committed change owes.
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
    /// Collects pending domain events from tracked aggregate roots into the
    /// per-context buffer and clears them from the aggregates. Clearing at
    /// collection time keeps aggregates clean whether the save later succeeds
    /// or fails, and keeps execution-strategy retries from re-buffering.
    /// </summary>
    /// <param name="context">The current DbContext instance.</param>
    private static void CollectDomainEvents(DbContext? context)
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

        // Contexts that do not map the outbox (tooling, focused tests) still dispatch in
        // process; only their durability guarantee is absent.
        bool hasOutbox = context.Model.FindEntityType(typeof(OutboxEventEntity)) is not null;

        foreach (IAggregate aggregate in aggregates)
        {
            foreach (IDomainEvent domainEvent in aggregate.ClearDomainEvents())
            {
                buffer.Add(domainEvent);

                if (!hasOutbox)
                {
                    continue;
                }

                // The row joins this same SaveChanges, so the event is durable exactly when the
                // state change is — a dispatch that dies after the commit replays from here.
                OutboxEventEntity row = OutboxEventEntity.Create(domainEvent);
                context.Set<OutboxEventEntity>().Add(row);
                outboxRows.Add(row);
            }
        }
    }

    /// <summary>
    /// Publishes the events buffered for the context, each in a fresh
    /// dependency injection scope so handlers never share the business
    /// operation's DbContext instance. Events publish in raise order; the
    /// buffer is removed before dispatch so reentrant saves start clean.
    /// Dispatch runs with <see cref="CancellationToken.None"/> because the
    /// reactions belong to the committed change, not to the request that
    /// triggered it. Scope creation and publisher resolution are guarded as
    /// well: the data is already durable at this point, so no dispatch
    /// failure may make the committed operation appear failed.
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
            // The enclosing transaction has not committed yet. The outbox rows carry the events
            // durably, so replay delivers them after the commit instead of publishing early.
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
    /// Writes the dispatch outcomes back to the outbox rows. Failures here are swallowed for the
    /// same reason dispatch failures are — the business operation already committed — and leave
    /// the rows undispatched, which the replay job is built to handle.
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
    /// Drops the events buffered for the context so a failed or canceled save
    /// dispatches nothing and leaves no stale events for a later save.
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
