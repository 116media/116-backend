using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Content.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz scheduled job that periodically purges abandoned article drafts.
/// </summary>
/// <remarks>
/// An abandoned draft is an article that satisfies all of the following:
/// <list type="bullet">
///   <item><c>Status = Draft</c></item>
///   <item><c>Body</c> is empty</item>
///   <item><c>Headline</c> is empty</item>
///   <item>Created more than <b>7 days</b> ago</item>
/// </list>
/// <para>
/// Cleanup order per draft:
/// <list type="number">
///   <item>Capture the body-image <c>article_images</c> storage keys on the aggregate. Cover-type
///   rows are excluded: their key belongs to the cover file row, purged by the file soft-delete
///   reaction.</item>
///   <item>Remove the article from the database — CASCADE deletes <c>article_images</c> rows automatically.</item>
///   <item>The committed removal dispatches the deletion fact; the shared cleanup handler
///   soft-deletes the cover file row and purges the remote assets post-commit.</item>
/// </list>
/// </para>
/// <para>
/// Schedule: runs once per hour via the cron expression registered in <c>ContentModule</c>.
/// Each draft is processed individually so a single failure does not abort the entire batch.
/// </para>
/// </remarks>
[DisallowConcurrentExecution]
public class AbandonedDraftCleanupJob(IServiceScopeFactory scopeFactory, ILogger<AbandonedDraftCleanupJob> logger)
    : IScheduledJob
{
    /// <summary>
    /// Age threshold after which an empty draft is considered abandoned.
    /// </summary>
    private static readonly TimeSpan AbandonedAfter = TimeSpan.FromDays(7);

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogDebug("AbandonedDraftCleanupJob triggered.");

        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            var articleRepository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IContentUnitOfWork>();

            DateTime cutoff = DateTime.UtcNow - AbandonedAfter;

            IReadOnlyList<ArticleEntity> drafts = await articleRepository.GetAbandonedDraftsAsync(
                cutoff: cutoff,
                cancellationToken: context.CancellationToken
            );

            if (drafts.Count == 0)
            {
                logger.LogDebug("No abandoned drafts found.");
                return;
            }

            logger.LogInformation("Found {Count} abandoned draft(s) to purge.", drafts.Count);

            foreach (ArticleEntity draft in drafts)
            {
                try
                {
                    List<string> bodyImageStorageKeys = draft
                        .Images.Where(i => i.ImageType == EnumArticleImageType.Body)
                        .Select(i => i.StorageKey)
                        .ToList();

                    draft.MarkDeleted(bodyImageStorageKeys: bodyImageStorageKeys);
                    articleRepository.Remove(draft);
                    await unitOfWork.CommitAsync(cancellationToken: context.CancellationToken);

                    logger.LogInformation("Purged abandoned draft {ArticleId}.", draft.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to purge abandoned draft {ArticleId}.", draft.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AbandonedDraftCleanupJob encountered an unexpected error.");
        }
    }
}
