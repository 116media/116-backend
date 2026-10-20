using _116.Mailer.Application.Shared.Ports;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Mailer.Infrastructure.BackgroundJobs;

/// <summary>
/// Triggers <see cref="OutboxEmailDispatcherJob" /> on the running scheduler.
/// </summary>
/// <param name="schedulerFactory">The Quartz scheduler factory.</param>
/// <param name="logger">The logger.</param>
public class QuartzOutboxDispatchSignal(ISchedulerFactory schedulerFactory, ILogger<QuartzOutboxDispatchSignal> logger)
    : IOutboxDispatchSignal
{
    /// <summary>
    /// The key the dispatcher is registered under.
    /// </summary>
    private static readonly JobKey DispatcherKey = new(nameof(OutboxEmailDispatcherJob));

    /// <inheritdoc />
    public async Task RequestDispatchAsync(CancellationToken cancellationToken)
    {
        try
        {
            IScheduler scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            await scheduler.TriggerJob(DispatcherKey, cancellationToken);
        }
        catch (Exception exception)
        {
            // The row is committed, so the cron still delivers it.
            logger.LogWarning(exception, "Immediate outbox dispatch could not be signalled.");
        }
    }
}
