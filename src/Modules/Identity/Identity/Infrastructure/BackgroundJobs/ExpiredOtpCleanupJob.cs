using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Shared.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Identity.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz scheduled job that periodically purges expired one-time passwords.
/// </summary>
/// <remarks>
/// <para>
/// An OTP row is purged once its expiry has passed, whether or not it was ever consumed: an
/// expired code can no longer be verified, so the row holds nothing the application reads.
/// Rows still within their expiry window are left untouched, including consumed ones, because
/// the re-validation path still matches against them.
/// </para>
/// <para>
/// The repository stages the removal on the tracked context and reports how many rows it
/// matched; the purge only reaches the database once this job commits the unit of work.
/// </para>
/// <para>
/// Schedule: runs once per hour via the cron expression registered in <c>IdentityModule</c>.
/// The purge is a single batch, so a failure aborts that run and leaves the rows for the next
/// one rather than deleting a partial set.
/// </para>
/// </remarks>
[DisallowConcurrentExecution]
public class ExpiredOtpCleanupJob(IServiceScopeFactory scopeFactory, ILogger<ExpiredOtpCleanupJob> logger)
    : IScheduledJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogDebug("ExpiredOtpCleanupJob triggered.");

        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            var otpRepository = scope.ServiceProvider.GetRequiredService<IOtpRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IIdentityUnitOfWork>();

            int purgedCount = await otpRepository.CleanupExpiredOtpsAsync(cancellationToken: context.CancellationToken);

            if (purgedCount == 0)
            {
                logger.LogDebug("No expired OTPs found.");
                return;
            }

            await unitOfWork.CommitAsync(cancellationToken: context.CancellationToken);

            logger.LogInformation("Purged {Count} expired OTP(s).", purgedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ExpiredOtpCleanupJob encountered an unexpected error.");
        }
    }
}
