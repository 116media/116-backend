using _116.Shared.Application.Jobs;
using _116.Shared.Application.Services;
using _116.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Shared.Infrastructure.Outbox;

/// <summary>
/// Re-dispatches domain events whose original dispatch died after the commit. The handler
/// pipeline's processed-event guard makes a replay safe even when the first attempt had already
/// completed some handlers.
/// </summary>
/// <typeparam name="TContext">The module context owning the outbox.</typeparam>
/// <param name="scopeFactory">Factory creating the scope each replay batch runs in.</param>
/// <param name="logger">Logger recording replay outcomes and dead rows.</param>
[DisallowConcurrentExecution]
public abstract class OutboxReplayJob<TContext>(IServiceScopeFactory scopeFactory, ILogger logger) : IScheduledJob
    where TContext : DbContext
{
    /// <summary>
    /// How many rows one run claims. Small batches keep a backlog from monopolizing the worker.
    /// </summary>
    private const int BatchSize = 50;

    /// <summary>
    /// After this many failed attempts a row stops being retried and is left for inspection.
    /// </summary>
    private const int MaxAttempts = 5;

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();

        List<OutboxEventEntity> pending = await dbContext
            .Set<OutboxEventEntity>()
            .AsTracking()
            .Where(row => row.DispatchedAt == null && row.AttemptCount < MaxAttempts)
            .OrderBy(row => row.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(context.CancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        logger.LogInformation("Replaying {Count} undispatched domain event(s).", pending.Count);

        foreach (OutboxEventEntity row in pending)
        {
            IDomainEvent? domainEvent = row.ToDomainEvent();

            if (domainEvent is null)
            {
                row.MarkFailed($"Event type '{row.EventType}' could not be resolved.");
                continue;
            }

            try
            {
                await publisher.Publish(domainEvent, context.CancellationToken);
                row.MarkDispatched();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Replaying domain event {EventId} failed.", row.Id);
                row.MarkFailed(exception.Message);
            }
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);

        int dead = pending.Count(row => row.DispatchedAt is null && row.AttemptCount >= MaxAttempts);
        if (dead > 0)
        {
            logger.LogError("{Count} domain event(s) exhausted their replay attempts.", dead);
        }
    }
}
