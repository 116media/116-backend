using _116.BuildingBlocks.Infrastructure.Jobs;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Content.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz scheduled job that reprojects every searchable type, so a document missed by a
/// dropped domain event is corrected without waiting for its source to be saved again.
/// The rebuild is an upsert per source row, so running it against a healthy projection
/// changes nothing.
/// </summary>
[DisallowConcurrentExecution]
public class SearchProjectionReconcileJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SearchProjectionReconcileJob> logger
) : IScheduledJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogDebug("SearchProjectionReconcileJob triggered.");

        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            var projection = scope.ServiceProvider.GetRequiredService<ISearchProjection>();

            IReadOnlyDictionary<EnumCoreContentType, int> written = await projection.RebuildAllAsync(
                context.CancellationToken
            );

            logger.LogInformation(
                "Reconciled {Total} search document(s) across {Types} type(s).",
                written.Values.Sum(),
                written.Count
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SearchProjectionReconcileJob encountered an unexpected error.");
        }
    }
}
