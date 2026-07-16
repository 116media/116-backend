using _116.Content.Application.Editorial.Builders;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IShortVideoRepository" /> for managing short video entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ShortVideoRepository(ContentDbContext context) : IShortVideoRepository
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
        IQueryable<ShortVideoEntity> query = context.ShortVideos.Include(s => s.ParentVideo);

        Specification<ShortVideoEntity>? spec = new ShortVideoQueryBuilder()
            .WithSearch(search: search)
            .WithIsActive(isActive: isActive)
            .Build();

        if (spec is not null)
        {
            query = query.ApplySpecification(specification: spec);
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
        IQueryable<ShortVideoEntity> query = context
            .ShortVideos.Include(shortVideo => shortVideo.ParentVideo)
            .Where(shortVideo => shortVideo.IsActive);

        if (afterSortKey is long afterKey)
        {
            query = query.Where(shortVideo => (shortVideo.FeedRank ^ seed) > afterKey);
        }

        return await query
            .OrderBy(shortVideo => shortVideo.FeedRank ^ seed)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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

        List<Guid> likedIds = await context
            .ShortVideoLikes.Where(like => like.UserId == userId && shortVideoIds.Contains(like.ShortVideoId))
            .Select(like => like.ShortVideoId)
            .ToListAsync(cancellationToken);

        List<Guid> bookmarkedIds = await context
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
        var specification = new ShortVideoLikeByUserIdSpecification(userId: userId);
        IQueryable<ShortVideoLikeEntity> query = context
            .ShortVideoLikes.ApplySpecification(specification: specification)
            .Where(like => like.ShortVideo.IsActive);
        int totalCount = await query.CountAsync(cancellationToken);
        List<ShortVideoLikeEntity> rows = await query
            .Include(like => like.ShortVideo)
                .ThenInclude(shortVideo => shortVideo.ParentVideo)
            .OrderByDescending(like => like.CreatedAt)
            .ThenByDescending(like => like.ShortVideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
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
        var specification = new ShortVideoBookmarkByUserIdSpecification(userId: userId);
        IQueryable<ShortVideoBookmarkEntity> query = context
            .ShortVideoBookmarks.ApplySpecification(specification: specification)
            .Where(bookmark => bookmark.ShortVideo.IsActive);
        int totalCount = await query.CountAsync(cancellationToken);
        List<ShortVideoBookmarkEntity> rows = await query
            .Include(bookmark => bookmark.ShortVideo)
                .ThenInclude(shortVideo => shortVideo.ParentVideo)
            .OrderByDescending(bookmark => bookmark.CreatedAt)
            .ThenByDescending(bookmark => bookmark.ShortVideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
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
        var specification = new ShortVideoShareByUserIdSpecification(userId: userId);
        var query = context
            .ShortVideoShares.ApplySpecification(specification: specification)
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
        Dictionary<Guid, ShortVideoEntity> shortVideos = await context
            .ShortVideos.Include(shortVideo => shortVideo.ParentVideo)
            .Where(shortVideo => shortVideoIds.Contains(shortVideo.Id))
            .AsNoTracking()
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
        var specification = new ShortVideoBySlugSpecification(slug: slug);
        return await context
            .ShortVideos.Include(s => s.ParentVideo)
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ShortVideoEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoByIdSpecification(id: id);
        return await context.ShortVideos.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<ShortVideoEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoByIdSpecification(id: id);
        return await context
            .ShortVideos.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(ShortVideoEntity shortVideo, CancellationToken cancellationToken = default)
    {
        await context.ShortVideos.AddAsync(shortVideo, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(ShortVideoEntity shortVideo)
    {
        context.ShortVideos.Update(shortVideo);
    }

    /// <inheritdoc />
    public void Remove(ShortVideoEntity shortVideo)
    {
        context.ShortVideos.Remove(shortVideo);
    }

    /// <inheritdoc />
    public async Task<bool> HasLikedAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoLikeByUserAndShortVideoSpecification(
            userId: userId,
            shortVideoId: shortVideoId
        );
        return await context
            .ShortVideoLikes.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddLikeAsync(ShortVideoLikeEntity like, CancellationToken cancellationToken = default)
    {
        await context.ShortVideoLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveLikeAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoLikeByUserAndShortVideoSpecification(
            userId: userId,
            shortVideoId: shortVideoId
        );
        ShortVideoLikeEntity? like = await context
            .ShortVideoLikes.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (like is not null)
        {
            context.ShortVideoLikes.Remove(like);
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasBookmarkedAsync(
        Guid userId,
        Guid shortVideoId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ShortVideoBookmarkByUserAndShortVideoSpecification(
            userId: userId,
            shortVideoId: shortVideoId
        );
        return await context
            .ShortVideoBookmarks.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddBookmarkAsync(ShortVideoBookmarkEntity bookmark, CancellationToken cancellationToken = default)
    {
        await context.ShortVideoBookmarks.AddAsync(bookmark, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveBookmarkAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoBookmarkByUserAndShortVideoSpecification(
            userId: userId,
            shortVideoId: shortVideoId
        );
        ShortVideoBookmarkEntity? bookmark = await context
            .ShortVideoBookmarks.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (bookmark is not null)
        {
            context.ShortVideoBookmarks.Remove(bookmark);
        }
    }

    /// <inheritdoc />
    public async Task AddShareAsync(ShortVideoShareEntity share, CancellationToken cancellationToken = default)
    {
        await context.ShortVideoShares.AddAsync(share, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddViewEventAsync(
        ShortVideoViewEventEntity viewEvent,
        CancellationToken cancellationToken = default
    )
    {
        await context.ShortVideoViewEvents.AddAsync(viewEvent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasCountedViewSinceAsync(
        Guid shortVideoId,
        string dedupKey,
        DateTime since,
        CancellationToken cancellationToken = default
    )
    {
        return await context.ShortVideoViewEvents.AnyAsync(
            x => x.ShortVideoId == shortVideoId && x.DedupKey == dedupKey && x.IsCounted && x.CreatedAt >= since,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<int> PruneUncountedViewEventsAsync(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return await context
            .ShortVideoViewEvents.Where(x => !x.IsCounted && x.CreatedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
