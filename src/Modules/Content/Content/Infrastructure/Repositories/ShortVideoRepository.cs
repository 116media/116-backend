using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IShortVideoRepository" /> for managing short video entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ShortVideoRepository(ContentDbContext context)
    : ContentRepository<ShortVideoEntity>(context),
        IShortVideoRepository
{
    /// <inheritdoc />
    public async Task<(List<ShortVideoEntity> ShortVideos, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ShortVideoEntity> query = Context.ShortVideos.Include(s => s.ParentVideo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(shortVideo => EF.Functions.ILike(shortVideo.Title, pattern));
        }

        if (isActive.HasValue)
        {
            query = query.Where(shortVideo => shortVideo.IsActive == isActive.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<ShortVideoEntity> shortVideos = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (shortVideos, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShortVideoEntity>> GetRandomizedFeedAsync(
        long seed,
        long? afterSortKey,
        int limit,
        CancellationToken cancellationToken = default
    )
    {
        // Each row's stable, uniformly-random FeedRank XORed with the session seed yields a
        // fresh uniform ordering per session (XOR-by-constant preserves uniformity), and the
        // unique FeedRank makes the sort key a strict total order — so keyset paging on it
        // alone never drifts or repeats, with no id tie-breaker needed.
        IQueryable<ShortVideoEntity> query = Context
            .ShortVideos.Include(shortVideo => shortVideo.ParentVideo)
            .Where(shortVideo => shortVideo.IsActive);

        if (afterSortKey is long afterKey)
        {
            query = query.Where(shortVideo => (shortVideo.FeedRank ^ seed) > afterKey);
        }

        return await query.OrderBy(shortVideo => shortVideo.FeedRank ^ seed).Take(limit).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlySet<Guid> Liked, IReadOnlySet<Guid> Bookmarked)> GetLikedAndBookmarkedIdsAsync(
        Guid? currentUserId,
        IReadOnlyCollection<Guid> shortVideoIds,
        CancellationToken cancellationToken = default
    )
    {
        if (currentUserId is not Guid userId || shortVideoIds.Count == 0)
        {
            return (new HashSet<Guid>(), new HashSet<Guid>());
        }

        List<Guid> likedIds = await Context
            .ShortVideoLikes.Where(like => like.UserId == userId && shortVideoIds.Contains(like.ShortVideoId))
            .Select(like => like.ShortVideoId)
            .ToListAsync(cancellationToken);

        List<Guid> bookmarkedIds = await Context
            .ShortVideoBookmarks.Where(bookmark =>
                bookmark.UserId == userId && shortVideoIds.Contains(bookmark.ShortVideoId)
            )
            .Select(bookmark => bookmark.ShortVideoId)
            .ToListAsync(cancellationToken);

        return (likedIds.ToHashSet(), bookmarkedIds.ToHashSet());
    }

    /// <inheritdoc />
    public async Task<(List<ShortVideoActivity> Items, int TotalCount)> GetLikedShortVideosAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ShortVideoLikeEntity> query = Context
            .ShortVideoLikes.Where(like => like.UserId == userId)
            .Where(like => like.ShortVideo.IsActive);
        int totalCount = await query.CountAsync(cancellationToken);
        List<ShortVideoLikeEntity> rows = await query
            .Include(like => like.ShortVideo)
                .ThenInclude(shortVideo => shortVideo.ParentVideo)
            .OrderByDescending(like => like.CreatedAt)
            .ThenByDescending(like => like.ShortVideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (
            rows.Select(row => new ShortVideoActivity(row.ShortVideo, row.CreatedAt ?? DateTime.MinValue)).ToList(),
            totalCount
        );
    }

    /// <inheritdoc />
    public async Task<(List<ShortVideoActivity> Items, int TotalCount)> GetBookmarkedShortVideosAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ShortVideoBookmarkEntity> query = Context
            .ShortVideoBookmarks.Where(bookmark => bookmark.UserId == userId)
            .Where(bookmark => bookmark.ShortVideo.IsActive);
        int totalCount = await query.CountAsync(cancellationToken);
        List<ShortVideoBookmarkEntity> rows = await query
            .Include(bookmark => bookmark.ShortVideo)
                .ThenInclude(shortVideo => shortVideo.ParentVideo)
            .OrderByDescending(bookmark => bookmark.CreatedAt)
            .ThenByDescending(bookmark => bookmark.ShortVideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (
            rows.Select(row => new ShortVideoActivity(row.ShortVideo, row.CreatedAt ?? DateTime.MinValue)).ToList(),
            totalCount
        );
    }

    /// <inheritdoc />
    public async Task<(List<ShortVideoActivity> Items, int TotalCount)> GetSharedShortVideosAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = Context
            .ShortVideoShares.Where(share => share.UserId == userId)
            .Where(share => share.ShortVideo.IsActive)
            .GroupBy(share => share.ShortVideoId)
            .Select(group => new
            {
                ShortVideoId = group.Key,
                LastInteractedAt = group.Max(share => share.CreatedAt),
                InteractionCount = group.Count(),
            });
        int totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(row => row.LastInteractedAt)
            .ThenByDescending(row => row.ShortVideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        List<Guid> shortVideoIds = rows.Select(row => row.ShortVideoId).ToList();
        Dictionary<Guid, ShortVideoEntity> shortVideos = await Context
            .ShortVideos.Include(shortVideo => shortVideo.ParentVideo)
            .Where(shortVideo => shortVideoIds.Contains(shortVideo.Id))
            .ToDictionaryAsync(shortVideo => shortVideo.Id, cancellationToken);

        return (
            rows.Select(row => new ShortVideoActivity(
                    shortVideos[row.ShortVideoId],
                    row.LastInteractedAt ?? DateTime.MinValue,
                    row.InteractionCount
                ))
                .ToList(),
            totalCount
        );
    }

    /// <inheritdoc />
    public async Task<ShortVideoEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Context
            .ShortVideos.Include(s => s.ParentVideo)
            .FirstOrDefaultAsync(shortVideo => shortVideo.Slug == slug, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasLikedAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        return await Context.ShortVideoLikes.AnyAsync(
            like => like.UserId == userId && like.ShortVideoId == shortVideoId,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddLikeAsync(ShortVideoLikeEntity like, CancellationToken cancellationToken = default)
    {
        await Context.ShortVideoLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveLikeAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        ShortVideoLikeEntity? like = await Context
            .ShortVideoLikes.AsTracking()
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ShortVideoId == shortVideoId, cancellationToken);

        if (like is not null)
        {
            like.MarkRemoved();
            Context.ShortVideoLikes.Remove(like);
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasBookmarkedAsync(
        Guid userId,
        Guid shortVideoId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context.ShortVideoBookmarks.AnyAsync(
            bookmark => bookmark.UserId == userId && bookmark.ShortVideoId == shortVideoId,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddBookmarkAsync(ShortVideoBookmarkEntity bookmark, CancellationToken cancellationToken = default)
    {
        await Context.ShortVideoBookmarks.AddAsync(bookmark, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveBookmarkAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        ShortVideoBookmarkEntity? bookmark = await Context
            .ShortVideoBookmarks.AsTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ShortVideoId == shortVideoId, cancellationToken);

        if (bookmark is not null)
        {
            bookmark.MarkRemoved();
            Context.ShortVideoBookmarks.Remove(bookmark);
        }
    }

    /// <inheritdoc />
    public async Task AddShareAsync(ShortVideoShareEntity share, CancellationToken cancellationToken = default)
    {
        await Context.ShortVideoShares.AddAsync(share, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddViewEventAsync(
        ShortVideoViewEventEntity viewEvent,
        CancellationToken cancellationToken = default
    )
    {
        await Context.ShortVideoViewEvents.AddAsync(viewEvent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasCountedViewSinceAsync(
        Guid shortVideoId,
        string dedupKey,
        DateTime since,
        CancellationToken cancellationToken = default
    )
    {
        return await Context.ShortVideoViewEvents.AnyAsync(
            x => x.ShortVideoId == shortVideoId && x.DedupKey == dedupKey && x.IsCounted && x.CreatedAt >= since,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<int> PruneUncountedViewEventsAsync(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return await Context
            .ShortVideoViewEvents.Where(x => !x.IsCounted && x.CreatedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int?> ApplyEngagementDeltaAsync(
        Guid shortVideoId,
        EnumEngagementKind kind,
        int delta,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ShortVideoEntity> row = Context.ShortVideos.Where(e => e.Id == shortVideoId);

        // Math.Max reaches PostgreSQL as GREATEST, so a racing unlike cannot go negative.
        return kind switch
        {
            EnumEngagementKind.Like => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.LikeCount, e => Math.Max(0, e.LikeCount + delta)),
                cancellationToken: cancellationToken
            ),
            EnumEngagementKind.Bookmark => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.BookmarkCount, e => Math.Max(0, e.BookmarkCount + delta)),
                cancellationToken: cancellationToken
            ),
            EnumEngagementKind.Share => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.ShareCount, e => Math.Max(0, e.ShareCount + delta)),
                cancellationToken: cancellationToken
            ),
            EnumEngagementKind.View => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.ViewCount, e => Math.Max(0, e.ViewCount + delta)),
                cancellationToken: cancellationToken
            ),
            _ => null,
        };
    }
}
