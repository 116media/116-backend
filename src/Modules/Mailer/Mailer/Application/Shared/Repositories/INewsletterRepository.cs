using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Shared.Application.Pagination;

namespace _116.Mailer.Application.Shared.Repositories;

/// <summary>
/// Persistence port for newsletter subscribers.
/// </summary>
public interface INewsletterRepository
{
    /// <summary>
    /// Adds a new subscriber to the current unit of work.
    /// </summary>
    /// <param name="subscriber">The subscriber to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(NewsletterSubscriberEntity subscriber, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a subscriber by email address (lowercased before comparison).
    /// </summary>
    /// <param name="email">The email address to look up.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The subscriber, or <c>null</c> when none exists.</returns>
    Task<NewsletterSubscriberEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a subscriber by confirmation token.
    /// </summary>
    /// <param name="token">The confirmation token from the emailed link.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The subscriber, or <c>null</c> when the token matches none.</returns>
    Task<NewsletterSubscriberEntity?> GetByConfirmationTokenAsync(string token, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a subscriber by unsubscribe token.
    /// </summary>
    /// <param name="token">The unsubscribe token from the emailed link.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The subscriber, or <c>null</c> when the token matches none.</returns>
    Task<NewsletterSubscriberEntity?> GetByUnsubscribeTokenAsync(string token, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a page of subscribers, newest first, optionally filtered by
    /// status.
    /// </summary>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The paginated subscribers.</returns>
    Task<PaginatedResult<NewsletterSubscriberEntity>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        EnumNewsletterStatus? status,
        CancellationToken cancellationToken
    );
}
