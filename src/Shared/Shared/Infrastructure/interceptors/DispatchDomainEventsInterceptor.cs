using System.Runtime.CompilerServices;
using _116.Shared.Application.Services;
using _116.Shared.Domain;
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
/// The dispatch point is the completion of the save, not the commit of an enclosing explicit
/// transaction. A save performed inside a <c>Database.BeginTransaction</c> block publishes while
/// that transaction is still open, so handlers would observe state no other connection can read
/// yet and a later rollback would leave the reactions behind. Domain events must therefore not be
/// raised by work that runs inside an explicit transaction.
/// </remarks>
public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private static readonly ConditionalWeakTable<DbContext, List<IDomainEvent>> BufferedEvents = new();

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

        foreach (IAggregate aggregate in aggregates)
        {
            buffer.AddRange(aggregate.ClearDomainEvents());
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

        BufferedEvents.Remove(context);

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            try
            {
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                var domainEventPublisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();

                await domainEventPublisher.Publish(domainEvent, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Dispatching domain event {EventType} failed after the save committed. Event: {DomainEvent}",
                    domainEvent.GetType().Name,
                    domainEvent
                );
            }
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
        }
    }
}
