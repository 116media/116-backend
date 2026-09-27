using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Mailer.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOutboxEmailRepository" />.
/// </summary>
/// <param name="context">The Mailer module database context.</param>
public class OutboxEmailRepository(MailerDbContext context)
    : MailerRepository<OutboxEmailEntity>(context),
        IOutboxEmailRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<OutboxEmailEntity>> ClaimDueBatchAsync(
        int batchSize,
        DateTime now,
        DateTime leaseExpiresAt,
        CancellationToken cancellationToken
    )
    {
        // Selecting and claiming in one statement is what lets the dispatcher send with no
        // transaction open: the claim is already durable, so provider calls hold no row locks.
        // FOR UPDATE SKIP LOCKED keeps concurrent replicas on disjoint batches, and the lapsed
        // lease returns rows a dispatcher died holding.
        return await Context
            .OutboxEmails.FromSqlInterpolated(
                $"""
                UPDATE mailer.outbox_emails
                SET status = {nameof(EnumOutboxEmailStatus.Claimed)}, lease_expires_at = {leaseExpiresAt}
                WHERE id IN (
                    SELECT id FROM mailer.outbox_emails
                    WHERE (status = {nameof(EnumOutboxEmailStatus.Pending)} AND next_attempt_at <= {now})
                       OR (status = {nameof(EnumOutboxEmailStatus.Claimed)} AND lease_expires_at <= {now})
                    ORDER BY next_attempt_at
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING *
                """
            )
            .ToListAsync(cancellationToken);
    }
}
