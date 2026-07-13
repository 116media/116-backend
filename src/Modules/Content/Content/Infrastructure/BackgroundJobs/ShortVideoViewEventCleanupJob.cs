using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.Repositories;
using _116.Shared.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Content.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz scheduled job that prunes raw short-video view events that did not increment the
/// displayed count once they age past <see cref="ViewCountingConstants.UncountedEventRetention" />.
/// Counted events are kept as the auditable basis of the displayed number; a future fraud
/// pass recomputing counts from raw events hooks in here.
/// </summary>
[DisallowConcurrentExecution]
public class ShortVideoViewEventCleanupJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ShortVideoViewEventCleanupJob> logger
) : IScheduledJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogDebug("ShortVideoViewEventCleanupJob triggered.");

        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            var shortVideoRepository = scope.ServiceProvider.GetRequiredService<IShortVideoRepository>();

            DateTime cutoff = DateTime.UtcNow - ViewCountingConstants.UncountedEventRetention;

            int removed = await shortVideoRepository.PruneUncountedViewEventsAsync(
                cutoff: cutoff,
                cancellationToken: context.CancellationToken
            );

            if (removed > 0)
            {
                logger.LogInformation("Pruned {Count} uncounted short-video view event(s).", removed);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ShortVideoViewEventCleanupJob encountered an unexpected error.");
        }
    }
}
