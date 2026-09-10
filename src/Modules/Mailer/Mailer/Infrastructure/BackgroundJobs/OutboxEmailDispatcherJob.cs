using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Mailer.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz scheduled job that delivers pending outbox emails through the
/// configured <see cref="IEmailSender" />.
/// </summary>
/// <remarks>
/// Every run claims a batch of due rows in one statement, performs one delivery attempt per row
/// with no transaction open, then writes the outcomes back:
/// <list type="bullet">
///   <item>success marks the row sent;</item>
///   <item>a transient failure schedules the next attempt from the backoff schedule;</item>
///   <item>a permanent failure (or an exhausted schedule) marks the row failed.</item>
/// </list>
/// One failing message never stops the batch, and a dispatcher that dies mid-batch leaves leases
/// that expire so the next run re-claims those rows. Schedule: every 15 seconds via the cron
/// expression registered in <c>MailerModule</c>.
/// </remarks>
[DisallowConcurrentExecution]
public class OutboxEmailDispatcherJob(IServiceScopeFactory scopeFactory, ILogger<OutboxEmailDispatcherJob> logger)
    : IScheduledJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MailerDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxEmailRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        DateTime now = DateTime.UtcNow;

        IReadOnlyList<OutboxEmailEntity> batch = await repository.ClaimDueBatchAsync(
            batchSize: MailerConstants.DispatchBatchSize,
            now: now,
            leaseExpiresAt: now + MailerConstants.ClaimLease,
            cancellationToken: context.CancellationToken
        );

        if (batch.Count == 0)
        {
            return;
        }

        foreach (OutboxEmailEntity email in batch)
        {
            await DeliverAsync(email, sender, context.CancellationToken);
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>
    /// Performs one delivery attempt for a claimed outbox email and records
    /// the outcome on the entity.
    /// </summary>
    private async Task DeliverAsync(OutboxEmailEntity email, IEmailSender sender, CancellationToken cancellationToken)
    {
        try
        {
            var message = new EmailMessage(
                To: new EmailRecipient(email.RecipientAddress, email.RecipientName),
                Subject: email.Subject,
                HtmlBody: email.HtmlBody,
                TextBody: email.TextBody
            );

            await sender.SendAsync(message, cancellationToken);

            email.MarkSent(DateTime.UtcNow);
            logger.LogInformation(
                "Outbox email {OutboxEmailId} ({Template}) sent after {AttemptCount} prior attempts.",
                email.Id,
                email.Template,
                email.AttemptCount
            );
        }
        catch (EmailDeliveryException exception)
        {
            email.RegisterFailure(exception.Message, exception.IsTransient, DateTime.UtcNow);
            logger.LogError(
                exception,
                "Outbox email {OutboxEmailId} ({Template}) delivery failed (transient: {IsTransient}, attempts: {AttemptCount}).",
                email.Id,
                email.Template,
                exception.IsTransient,
                email.AttemptCount
            );
        }
        catch (Exception exception)
        {
            email.RegisterFailure(exception.Message, isTransient: true, DateTime.UtcNow);
            logger.LogError(
                exception,
                "Outbox email {OutboxEmailId} ({Template}) delivery threw unexpectedly (attempts: {AttemptCount}).",
                email.Id,
                email.Template,
                email.AttemptCount
            );
        }
    }
}
