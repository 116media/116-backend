using _116.Shared.Application.Services;
using _116.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Infrastructure.interceptors;

/// <summary>
/// EF Core interceptor that automatically publishes domain events before saving changes to the database.
/// Finds all aggregate entities with pending domain events, publishes them, and clears the events.
/// </summary>
public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatchDomainEventsInterceptor"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Factory to create scoped services for domain event publishing.</param>
    public DispatchDomainEventsInterceptor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Intercepts the synchronous save operation and dispatches domain events before saving.
    /// </summary>
    /// <param name="eventData">Contextual information about the save operation.</param>
    /// <param name="result">The interception result.</param>
    /// <returns>The modified interception result.</returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts the asynchronous save operation and dispatches domain events before saving.
    /// </summary>
    /// <param name="eventData">Contextual information about the save operation.</param>
    /// <param name="result">The interception result.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the interception result.</returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Collects and publishes domain events from tracked aggregate roots.
    /// </summary>
    /// <param name="eventDataContext">The current DbContext instance.</param>
    /// <returns>A task that completes once all domain events are published.</returns>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    /// <item>Finds all aggregates implementing <see cref="IAggregate"/> with pending domain events.</item>
    /// <item>Collects and flattens all domain events into a single list.</item>
    /// <item>Clears domain events from the aggregate roots.</item>
    /// <item>Publishes each event using the domain event publisher.</item>
    /// </list>
    /// </remarks>
    private async Task DispatchDomainEvents(DbContext? eventDataContext)
    {
        if (eventDataContext == null)
        {
            return;
        }

        // Step 1: Get aggregates with domain events
        List<IAggregate> aggregates = eventDataContext
            .ChangeTracker.Entries<IAggregate>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        // Step 2: Extract and flatten domain events
        List<IDomainEvent> domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        // Step 3: Clear domain events from aggregates
        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

        // Step 4: Publish each event using scoped domain event publisher
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        var domainEventPublisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await domainEventPublisher.Publish(domainEvent);
        }
    }
}
