using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using Microsoft.EntityFrameworkCore;

namespace _116.Mailer.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="INotificationRepository" />.
/// </summary>
/// <param name="context">The Mailer module database context.</param>
public class NotificationRepository(MailerDbContext context) : INotificationRepository
{
    /// <inheritdoc />
    public async Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken)
    {
        await context.Notifications.AddAsync(notification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<NotificationEntity>> GetPagedForUserAsync(
        Guid userId,
        int pageIndex,
        int pageSize,
        bool unreadOnly,
        CancellationToken cancellationToken
    )
    {
        IQueryable<NotificationEntity> query = context.Notifications.AsNoTracking().Where(x => x.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(x => x.ReadAt == null);
        }

        long count = await query.LongCountAsync(cancellationToken);
        List<NotificationEntity> items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<NotificationEntity>(pageIndex, pageSize, count, items);
    }

    /// <inheritdoc />
    public async Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Notifications.CountAsync(x => x.UserId == userId && x.ReadAt == null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NotificationEntity?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await context.Notifications.FirstOrDefaultAsync(
            x => x.Id == id && x.UserId == userId,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationEntity>> GetUnreadForUserAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        return await context
            .Notifications.Where(x => x.UserId == userId && x.ReadAt == null)
            .ToListAsync(cancellationToken);
    }
}
