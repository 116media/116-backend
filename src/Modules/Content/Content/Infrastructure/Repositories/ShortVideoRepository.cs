using _116.Content.Application.Editorial.Builders;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Specifications;
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
        IQueryable<ShortVideoEntity> query = Context.ShortVideos;

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
        IQueryable<ShortVideoEntity> query = Context.ShortVideos.ApplySpecification(
            specification: new ActiveShortVideoSpecification()
        );

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
            .ShortVideoLikes.ApplySpecification(specification: new ShortVideoLikeByUserIdSpecification(userId))
            .Where(like => shortVideoIds.Contains(like.ShortVideoId))
            .Select(like => like.ShortVideoId)
            .ToListAsync(cancellationToken);

        List<Guid> bookmarkedIds = await Context
            .ShortVideoBookmarks.ApplySpecification(specification: new ShortVideoBookmarkByUserIdSpecification(userId))
            .Where(bookmark => shortVideoIds.Contains(bookmark.ShortVideoId))
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
        IQueryable<ShortVideoLikeEntity> query = Context
            .ShortVideoLikes.ApplySpecification(specification: specification)
            .Where(like =>
                Context.ShortVideos.Any(shortVideo => shortVideo.Id == like.ShortVideoId && shortVideo.IsActive)
            );
        int totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(like => like.CreatedAt)
            .ThenByDescending(like => like.ShortVideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(like => new { like.ShortVideoId, like.CreatedAt })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, ShortVideoEntity> shortVideos = await LoadShortVideosAsync(
            [.. rows.Select(row => row.ShortVideoId)],
            cancellationToken
        );

        return (
            rows.Select(row => new ShortVideoActivity(
                    shortVideos[row.ShortVideoId],
                    row.CreatedAt ?? DateTime.MinValue
                ))
                .ToList(),
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
        IQueryable<ShortVideoBookmarkEntity> query = Context
            .ShortVideoBookmarks.ApplySpecification(specification: specification)
            .Where(bookmark =>
                Context.ShortVideos.Any(shortVideo => shortVideo.Id == bookmark.ShortVideoId && shortVideo.IsActive)
            );
        int totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(bookmark => bookmark.CreatedAt)
            .ThenByDescending(bookmark => bookmark.ShortVideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(bookmark => new { bookmark.ShortVideoId, bookmark.CreatedAt })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, ShortVideoEntity> shortVideos = await LoadShortVideosAsync(
            [.. rows.Select(row => row.ShortVideoId)],
            cancellationToken
        );

        return (
            rows.Select(row => new ShortVideoActivity(
                    shortVideos[row.ShortVideoId],
                    row.CreatedAt ?? DateTime.MinValue
                ))
                .ToList(),
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
        var query = Context
            .ShortVideoShares.ApplySpecification(specification: specification)
            .Where(share =>
                Context.ShortVideos.Any(shortVideo => shortVideo.Id == share.ShortVideoId && shortVideo.IsActive)
            )
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
        Dictionary<Guid, ShortVideoEntity> shortVideos = await LoadShortVideosAsync(
            [.. rows.Select(row => row.ShortVideoId)],
            cancellationToken
        );

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
        return await Context
            .ShortVideos.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasLikedAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoLikeByUserAndShortVideoSpecification(
            userId: userId,
            shortVideoId: shortVideoId
        );
        return await Context
            .ShortVideoLikes.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddLikeAsync(ShortVideoLikeEntity like, CancellationToken cancellationToken = default)
    {
        await Context.ShortVideoLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveLikeAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoLikeByUserAndShortVideoSpecification(
            userId: userId,
            shortVideoId: shortVideoId
        );
        ShortVideoLikeEntity? like = await Context
            .ShortVideoLikes.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

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
        var specification = new ShortVideoBookmarkByUserAndShortVideoSpecification(
            userId: userId,
            shortVideoId: shortVideoId
        );
        return await Context
            .ShortVideoBookmarks.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddBookmarkAsync(ShortVideoBookmarkEntity bookmark, CancellationToken cancellationToken = default)
    {
        await Context.ShortVideoBookmarks.AddAsync(bookmark, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveBookmarkAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoBookmarkByUserAndShortVideoSpecification(
            userId: userId,
            shortVideoId: shortVideoId
        );
        ShortVideoBookmarkEntity? bookmark = await Context
            .ShortVideoBookmarks.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

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
        return await Context.ShortVideoViewEvents.AnyBySpecificationAsync(
            specification: new ShortVideoCountedViewSinceSpecification(
                shortVideoId: shortVideoId,
                dedupKey: dedupKey,
                since: since
            ),
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<int> PruneUncountedViewEventsAsync(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return await Context
            .ShortVideoViewEvents.ApplySpecification(
                specification: new UncountedShortVideoViewBeforeSpecification(cutoff: cutoff)
            )
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
        IQueryable<ShortVideoEntity> row = Context.ShortVideos.ApplySpecification(
            specification: new ShortVideoByIdSpecification(id: shortVideoId)
        );

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

    /// <summary>
    /// Loads the short videos a page of interaction rows points at, keyed by id.
    /// </summary>
    /// <param name="shortVideoIds">The short video ids on the page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The short videos by id.</returns>
    private async Task<Dictionary<Guid, ShortVideoEntity>> LoadShortVideosAsync(
        Guid[] shortVideoIds,
        CancellationToken cancellationToken
    )
    {
        if (shortVideoIds.Length == 0)
        {
            return [];
        }

        return await Context
            .ShortVideos.Where(shortVideo => shortVideoIds.Contains(shortVideo.Id))
            .ToDictionaryAsync(shortVideo => shortVideo.Id, cancellationToken);
    }
}
