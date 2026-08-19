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
public class OutboxEmailRepository(MailerDbContext context) : IOutboxEmailRepository
{
    /// <inheritdoc />
    public async Task AddAsync(OutboxEmailEntity email, CancellationToken cancellationToken)
    {
        await context.OutboxEmails.AddAsync(email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OutboxEmailEntity>> ClaimDueBatchAsync(
        int batchSize,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        // FOR UPDATE SKIP LOCKED keeps concurrent dispatchers (multiple API
        // replicas) from double-sending the same row: each claims a disjoint
        // batch and the losers skip instead of blocking. The lock only holds
        // while the dispatcher's surrounding transaction is open.
        return await context
            .OutboxEmails.FromSqlInterpolated(
                $"""
                SELECT * FROM mailer.outbox_emails
                WHERE status = {nameof(EnumOutboxEmailStatus.Pending)} AND next_attempt_at <= {now}
                ORDER BY next_attempt_at
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """
            )
            .ToListAsync(cancellationToken);
    }
}
