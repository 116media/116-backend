using _116.Mailer.Application.Newsletter.Services;
using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application.DTOs;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace _116.Mailer.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz scheduled job that delivers pending outbox emails through the
/// configured <see cref="IEmailSenderService" />.
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
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();

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
            await DeliverAsync(email, sender, dbContext, context.CancellationToken);
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>
    /// Builds the one-click unsubscribe headers for a recipient who is a newsletter subscriber.
    /// Returns null for everyone else, so transactional mail carries no unsubscribe affordance.
    /// </summary>
    /// <param name="recipientAddress">The address the outbox row is addressed to.</param>
    /// <param name="dbContext">The Mailer context the subscriber is read from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The headers to attach, or null.</returns>
    private static async Task<IReadOnlyDictionary<string, string>?> ResolveUnsubscribeHeadersAsync(
        string recipientAddress,
        MailerDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        string? token = await dbContext
            .NewsletterSubscribers.Where(subscriber => subscriber.Email == recipientAddress)
            .Select(subscriber => subscriber.UnsubscribeToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return new Dictionary<string, string>
        {
            ["List-Unsubscribe"] = $"<{NewsletterLinkBuilder.UnsubscribeUrl(token)}>",
            ["List-Unsubscribe-Post"] = "List-Unsubscribe=One-Click",
        };
    }

    /// <summary>
    /// Performs one delivery attempt for a claimed outbox email and records
    /// the outcome on the entity.
    /// </summary>
    private async Task DeliverAsync(
        OutboxEmailEntity email,
        IEmailSenderService sender,
        MailerDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var message = new EmailMessage(
                To: new EmailRecipientDto(email.RecipientAddress, email.RecipientName),
                Subject: email.Subject,
                HtmlBody: email.HtmlBody,
                TextBody: email.TextBody,
                Headers: await ResolveUnsubscribeHeadersAsync(email.RecipientAddress, dbContext, cancellationToken)
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
