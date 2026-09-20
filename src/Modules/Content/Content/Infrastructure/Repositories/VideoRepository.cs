using _116.Content.Application.Editorial.Builders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IVideoRepository" /> for managing video entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class VideoRepository(ContentDbContext context) : ContentRepository<VideoEntity>(context), IVideoRepository
{
    /// <inheritdoc />
    public async Task<(List<VideoEntity> Videos, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        EnumContentStatus? status,
        Guid? categoryId,
        string? tagSlug = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<VideoEntity> query = Context.Videos.Include(v => v.Category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(video =>
                EF.Functions.ILike(video.Title, pattern)
                || (video.Description != null && EF.Functions.ILike(video.Description, pattern))
                || (video.MetaTitle != null && EF.Functions.ILike(video.MetaTitle, pattern))
                || (video.MetaDescription != null && EF.Functions.ILike(video.MetaDescription, pattern))
            );
        }

        if (status.HasValue)
        {
            query = query.Where(video => video.Status == status.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(video => video.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tagSlug))
        {
            query = query.Where(video => video.Tags.Any(videoTag => EF.Functions.ILike(videoTag.Tag.Slug, tagSlug)));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<VideoEntity> videos = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (videos, totalCount);
    }

    /// <inheritdoc />
    public async Task<List<VideoEntity>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Context
            .Videos.Include(v => v.Category)
            .Where(video => video.Status != EnumContentStatus.Archived && video.Status != EnumContentStatus.Rejected)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<VideoEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context
            .Videos.Where(video => video.Id == id)
            .Include(v => v.Category)
            .Include(v => v.Tags)
                .ThenInclude(t => t.Tag)
            .Include(v => v.Customer)
            .Include(v => v.PromotionLevel)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<VideoEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context
            .Videos.AsTracking()
            .Where(video => video.Id == id)
            .Include(v => v.Category)
            .Include(v => v.Tags)
                .ThenInclude(t => t.Tag)
            .Include(v => v.Customer)
            .Include(v => v.PromotionLevel)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VideoEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Context
            .Videos.AsTracking()
            .Where(video => EF.Functions.ILike(video.Slug, slug))
            .Include(v => v.Category)
            .Include(v => v.Tags)
                .ThenInclude(t => t.Tag)
            .Include(v => v.PromotionLevel)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoEntity>> GetPromotedAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return await Context
            .Videos.Where(video =>
                video.IsPromoted
                && video.Status == EnumContentStatus.Published
                && (video.PromotedUntil == null || video.PromotedUntil > now)
            )
            .Include(v => v.Category)
            .OrderByDescending(v => v.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoEntity>> GetPopularVideosAsync(
        int limit,
        Guid? categoryId,
        Guid? excludeId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<VideoEntity> query = new PopularVideosQueryBuilder()
            .WithCategory(categoryId: categoryId)
            .WithExcludeId(excludeId: excludeId)
            .WithLimit(limit: limit)
            .Build(source: Context.Videos.Include(v => v.Category));

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VideoEntity?> GetByOrderItemIdAsync(
        Guid orderItemId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .Videos.AsTracking()
            .FirstOrDefaultAsync(video => video.OrderItemId == orderItemId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddTagAsync(VideoTagEntity tag, CancellationToken cancellationToken = default)
    {
        await Context.VideoTags.AddAsync(tag, cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveTag(VideoTagEntity tag)
    {
        tag.MarkRemoved();
        Context.VideoTags.Remove(tag);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoTagEntity>> GetTagsByVideoIdAsync(
        Guid videoId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context.VideoTags.AsTracking().Where(tag => tag.VideoId == videoId).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VideoRatingEntity?> GetRatingAsync(
        Guid userId,
        Guid videoId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .VideoRatings.AsTracking()
            .FirstOrDefaultAsync(rating => rating.UserId == userId && rating.VideoId == videoId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRatingAsync(VideoRatingEntity rating, CancellationToken cancellationToken = default)
    {
        await Context.VideoRatings.AddAsync(rating, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<VideoRatingEntity>> GetAllRatingsForVideoAsync(
        Guid videoId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context.VideoRatings.Where(rating => rating.VideoId == videoId).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddShareAsync(VideoShareEntity share, CancellationToken cancellationToken = default)
    {
        await Context.VideoShares.AddAsync(share, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<RatedVideoActivity> Activities, int TotalCount)> GetRatedVideosByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<VideoRatingEntity> query = Context
            .VideoRatings.Where(rating => rating.UserId == userId)
            .Where(rating => rating.Video.Status == EnumContentStatus.Published);

        int totalCount = await query.CountAsync(cancellationToken);
        List<VideoRatingEntity> ratings = await query
            .Include(rating => rating.Video)
                .ThenInclude(video => video.Category)
            .OrderByDescending(rating => rating.UpdatedAt ?? rating.CreatedAt)
            .ThenBy(rating => rating.VideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        IReadOnlyList<RatedVideoActivity> activities = ratings
            .Select(rating => new RatedVideoActivity(
                Video: rating.Video,
                Stars: rating.Stars,
                LastInteractedAt: rating.UpdatedAt ?? rating.CreatedAt ?? DateTime.MinValue
            ))
            .ToList();

        return (activities, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<SharedVideoActivity> Activities, int TotalCount)> GetSharedVideosByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<VideoShareEntity> ownPublishedShares = Context
            .VideoShares.Where(share => share.UserId == userId)
            .Where(share => share.Video.Status == EnumContentStatus.Published);

        int totalCount = await ownPublishedShares
            .Select(share => share.VideoId)
            .Distinct()
            .CountAsync(cancellationToken);

        var pageMetadata = await ownPublishedShares
            .GroupBy(share => share.VideoId)
            .Select(group => new
            {
                VideoId = group.Key,
                ShareCount = group.Count(),
                LastInteractedAt = group.Max(share => share.CreatedAt) ?? DateTime.MinValue,
                LastShareChannel = group
                    .OrderByDescending(share => share.CreatedAt)
                    .ThenByDescending(share => share.Id)
                    .Select(share => share.ShareChannel)
                    .FirstOrDefault(),
            })
            .OrderByDescending(activity => activity.LastInteractedAt)
            .ThenBy(activity => activity.VideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        Guid[] videoIds = pageMetadata.Select(activity => activity.VideoId).ToArray();
        Dictionary<Guid, VideoEntity> videos = await Context
            .Videos.Where(video => videoIds.Contains(video.Id))
            .Include(video => video.Category)
            .ToDictionaryAsync(video => video.Id, cancellationToken);

        IReadOnlyList<SharedVideoActivity> activities = pageMetadata
            .Select(activity => new SharedVideoActivity(
                Video: videos[activity.VideoId],
                ShareCount: activity.ShareCount,
                LastInteractedAt: activity.LastInteractedAt,
                LastShareChannel: activity.LastShareChannel
            ))
            .ToList();

        return (activities, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoEntity>> GetActivePromotedBySpotAsync(
        int spotPriority,
        CancellationToken cancellationToken = default
    )
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return await Context
            .Videos.Where(video =>
                video.IsPromoted
                && video.Status == EnumContentStatus.Published
                && (video.PromotedUntil == null || video.PromotedUntil > now)
                && video.PromotionLevel != null
                && video.PromotionLevel.SpotPriority == spotPriority
            )
            .Include(v => v.Category)
            .Include(v => v.PromotionLevel)
            .OrderByDescending(v => v.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoEntity>> GetFreeVideosAsync(
        int limit,
        IEnumerable<Guid> excludeIds,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .Videos.Where(video => video.Status == EnumContentStatus.Published && video.CustomerId == null)
            .Where(v => !excludeIds.Contains(v.Id))
            .Include(v => v.Category)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoEntity>> GetLatestPublishedByCategoryAsync(
        Guid categoryId,
        int limit,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .Videos.Where(video => video.Status == EnumContentStatus.Published && video.CategoryId == categoryId)
            .Include(v => v.Category)
            .OrderByDescending(v => v.PublishedAt)
            .ThenByDescending(v => v.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountPublishedByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await Context
            .Videos.Where(video => video.Status == EnumContentStatus.Published && video.CategoryId == categoryId)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<VideoEntity> Videos, int TotalCount)> GetPublishedByArtistAsync(
        Guid artistId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<VideoEntity> query = Context
            .Videos.Include(v => v.Category)
            .Where(video => video.Status == EnumContentStatus.Published && video.ArtistId == artistId);

        int totalCount = await query.CountAsync(cancellationToken);

        List<VideoEntity> videos = await query
            .OrderByDescending(v => v.PublishedAt)
            .ThenByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (videos, totalCount);
    }

    /// <inheritdoc />
    public async Task<int?> ApplyEngagementDeltaAsync(
        Guid videoId,
        EnumEngagementKind kind,
        int delta,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<VideoEntity> row = Context.Videos.Where(e => e.Id == videoId);

        // Math.Max reaches PostgreSQL as GREATEST, so a racing unlike cannot go negative.
        return kind switch
        {
            EnumEngagementKind.Share => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.ShareCount, e => Math.Max(0, e.ShareCount + delta)),
                cancellationToken: cancellationToken
            ),
            _ => null,
        };
    }

    /// <inheritdoc />
    public Task<int> SetRatingAsync(
        Guid videoId,
        decimal average,
        int count,
        CancellationToken cancellationToken = default
    )
    {
        return Context
            .Videos.Where(v => v.Id == videoId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(v => v.RatingAverage, average).SetProperty(v => v.RatingCount, count),
                cancellationToken: cancellationToken
            );
    }
}
