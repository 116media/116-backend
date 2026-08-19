using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using Microsoft.EntityFrameworkCore;

namespace _116.Mailer.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="INewsletterRepository" />.
/// </summary>
/// <param name="context">The Mailer module database context.</param>
public class NewsletterRepository(MailerDbContext context) : INewsletterRepository
{
    /// <inheritdoc />
    public async Task AddAsync(NewsletterSubscriberEntity subscriber, CancellationToken cancellationToken)
    {
        await context.NewsletterSubscribers.AddAsync(subscriber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NewsletterSubscriberEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        string normalized = email.Trim().ToLowerInvariant();
        return await context.NewsletterSubscribers.FirstOrDefaultAsync(x => x.Email == normalized, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NewsletterSubscriberEntity?> GetByConfirmationTokenAsync(
        string token,
        CancellationToken cancellationToken
    )
    {
        return await context.NewsletterSubscribers.FirstOrDefaultAsync(
            x => x.ConfirmationToken == token,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<NewsletterSubscriberEntity?> GetByUnsubscribeTokenAsync(
        string token,
        CancellationToken cancellationToken
    )
    {
        return await context.NewsletterSubscribers.FirstOrDefaultAsync(
            x => x.UnsubscribeToken == token,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<NewsletterSubscriberEntity>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        EnumNewsletterStatus? status,
        CancellationToken cancellationToken
    )
    {
        IQueryable<NewsletterSubscriberEntity> query = context.NewsletterSubscribers.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        long count = await query.LongCountAsync(cancellationToken);
        List<NewsletterSubscriberEntity> items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<NewsletterSubscriberEntity>(pageIndex, pageSize, count, items);
    }
}
