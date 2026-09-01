using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Constants;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Core.Infrastructure.BackgroundJobs;

/// <summary>
/// Removes uploads that were never claimed by a referencing write, together with the remote
/// assets behind them.
/// </summary>
/// <remarks>
/// An upload and the row pointing at it belong to different modules and cannot share a
/// transaction, so a referencing write that fails leaves the file row behind. Claiming makes that
/// state observable and this job makes it self-healing. The grace period is far longer than any
/// request, so a claim in flight is never reaped out from under itself.
/// </remarks>
/// <param name="scopeFactory">Factory creating the scope each reap batch runs in.</param>
/// <param name="logger">Logger recording what was reclaimed.</param>
[DisallowConcurrentExecution]
public class UnclaimedFileReaperJob(IServiceScopeFactory scopeFactory, ILogger<UnclaimedFileReaperJob> logger)
    : IScheduledJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();

        DateTime olderThan = DateTime.UtcNow - CoreConstants.UnclaimedFileGracePeriod;

        IReadOnlyList<FileEntity> abandoned = await fileRepository.GetUnclaimedBeforeAsync(
            olderThan: olderThan,
            batchSize: CoreConstants.UnclaimedFileReapBatchSize,
            cancellationToken: context.CancellationToken
        );

        if (abandoned.Count == 0)
        {
            return;
        }

        logger.LogInformation("Reaping {Count} upload(s) that were never referenced.", abandoned.Count);

        foreach (FileEntity file in abandoned)
        {
            // Soft-deleting raises the file's own cleanup event, so the remote asset is removed
            // by the same handler that clears deliberately deleted files.
            file.Delete();
        }

        await fileRepository.SaveChangesAsync(context.CancellationToken);
    }
}
