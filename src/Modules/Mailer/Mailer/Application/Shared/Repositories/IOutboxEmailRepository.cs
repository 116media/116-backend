using _116.Mailer.Domain.Entities;

namespace _116.Mailer.Application.Shared.Repositories;

/// <summary>
/// Persistence port for outbox emails.
/// </summary>
public interface IOutboxEmailRepository
{
    /// <summary>
    /// Adds a new outbox email to the current unit of work.
    /// </summary>
    /// <param name="email">The pending outbox email to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(OutboxEmailEntity email, CancellationToken cancellationToken);

    /// <summary>
    /// Claims the next batch of due pending emails for delivery, ordered by
    /// next attempt time. Rows are locked with skip-locked semantics so
    /// concurrent dispatchers never double-send.
    /// </summary>
    /// <param name="batchSize">Maximum number of emails to claim.</param>
    /// <param name="now">The current UTC time; only rows due by now are claimed.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The claimed pending emails, oldest due first.</returns>
    Task<IReadOnlyList<OutboxEmailEntity>> ClaimDueBatchAsync(
        int batchSize,
        DateTime now,
        CancellationToken cancellationToken
    );
}
