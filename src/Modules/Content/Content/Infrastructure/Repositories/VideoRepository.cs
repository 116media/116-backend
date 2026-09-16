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
        IQueryable<VideoEntity> query = Context.Videos;

        Specification<VideoEntity>? spec = new VideoQueryBuilder()
            .WithSearch(search: search)
            .WithStatus(status: status)
            .WithCategory(categoryId: categoryId)
            .WithTag(tagSlug: tagSlug)
            .Build(tags: Context.Tags);

        if (spec is not null)
        {
            query = query.ApplySpecification(specification: spec);
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
        var specification = new ActiveVideoSpecification();
        return await Context
            .Videos.ApplySpecification(specification)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override IQueryable<VideoEntity> Query()
    {
        return Context.Videos.Include(v => v.Tags);
    }

    /// <inheritdoc />
    public async Task<VideoEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new VideoBySlugSpecification(slug: slug);
        return await Context
            .Videos.AsTracking()
            .ApplySpecification(specification: specification)
            .Include(v => v.Tags)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoEntity>> GetPromotedAsync(CancellationToken cancellationToken = default)
    {
        var specification = new PromotedVideoSpecification();
        return await Context
            .Videos.ApplySpecification(specification: specification)
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
            .Build(source: Context.Videos);

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VideoEntity?> GetByOrderItemIdAsync(
        Guid orderItemId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new VideoByOrderItemIdSpecification(orderItemId: orderItemId);
        return await Context
            .Videos.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VideoRatingEntity?> GetRatingAsync(
        Guid userId,
        Guid videoId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new VideoRatingByUserAndVideoSpecification(userId: userId, videoId: videoId);
        return await Context
            .VideoRatings.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
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
        var specification = new VideoRatingByVideoIdSpecification(videoId: videoId);
        return await Context
            .VideoRatings.ApplySpecification(specification: specification)
            .ToListAsync(cancellationToken);
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
        var specification = new VideoRatingByUserIdSpecification(userId: userId);
        IQueryable<VideoRatingEntity> query = Context
            .VideoRatings.ApplySpecification(specification: specification)
            .Where(rating =>
                Context.Videos.Any(video => video.Id == rating.VideoId && video.Status == EnumContentStatus.Published)
            );

        int totalCount = await query.CountAsync(cancellationToken);
        var pageRows = await query
            .OrderByDescending(rating => rating.UpdatedAt ?? rating.CreatedAt)
            .ThenBy(rating => rating.VideoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(rating => new
            {
                rating.VideoId,
                rating.Stars,
                LastInteractedAt = rating.UpdatedAt ?? rating.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, VideoEntity> videos = await LoadVideosAsync(
            [.. pageRows.Select(row => row.VideoId)],
            cancellationToken
        );

        IReadOnlyList<RatedVideoActivity> activities = pageRows
            .Select(row => new RatedVideoActivity(
                Video: videos[row.VideoId],
                Stars: row.Stars,
                LastInteractedAt: row.LastInteractedAt ?? DateTime.MinValue
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
        var specification = new VideoShareByUserIdSpecification(userId: userId);
        IQueryable<VideoShareEntity> ownPublishedShares = Context
            .VideoShares.ApplySpecification(specification: specification)
            .Where(share =>
                Context.Videos.Any(video => video.Id == share.VideoId && video.Status == EnumContentStatus.Published)
            );

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

        Dictionary<Guid, VideoEntity> videos = await LoadVideosAsync(
            [.. pageMetadata.Select(activity => activity.VideoId)],
            cancellationToken
        );

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
        var specification = new VideoBySpotPrioritySpecification(spotPriority: spotPriority, Context.PromotionLevels);
        return await Context
            .Videos.ApplySpecification(specification: specification)
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
        var specification = new FreeVideoSpecification();
        return await Context
            .Videos.ApplySpecification(specification: specification)
            .Where(v => !excludeIds.Contains(v.Id))
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
        Specification<VideoEntity> specification = new VideoByStatusSpecification(EnumContentStatus.Published).And(
            new VideoByCategorySpecification(categoryId: categoryId)
        );

        return await Context
            .Videos.ApplySpecification(specification: specification)
            .OrderByDescending(v => v.PublishedAt)
            .ThenByDescending(v => v.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountPublishedByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        Specification<VideoEntity> specification = new VideoByStatusSpecification(EnumContentStatus.Published).And(
            new VideoByCategorySpecification(categoryId: categoryId)
        );

        return await Context.Videos.ApplySpecification(specification: specification).CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<VideoEntity> Videos, int TotalCount)> GetPublishedByArtistAsync(
        Guid artistId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        Specification<VideoEntity> specification = new VideoByStatusSpecification(EnumContentStatus.Published).And(
            new VideoByArtistSpecification(artistId: artistId)
        );

        IQueryable<VideoEntity> query = Context.Videos.ApplySpecification(specification: specification);

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
        IQueryable<VideoEntity> row = Context.Videos.ApplySpecification(
            specification: new VideoByIdSpecification(id: videoId)
        );

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
            .Videos.ApplySpecification(specification: new VideoByIdSpecification(id: videoId))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(v => v.RatingAverage, average).SetProperty(v => v.RatingCount, count),
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public Task<bool> HasPublishedLyricsAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        return Context
            .Lyrics.ApplySpecification(specification: new LyricsByVideoIdSpecification(videoId: videoId))
            .ApplySpecification(specification: new LyricsByStatusSpecification(status: EnumContentStatus.Published))
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlySet<Guid>> GetIdsWithPublishedLyricsAsync(
        IReadOnlyCollection<Guid> videoIds,
        CancellationToken cancellationToken = default
    )
    {
        List<Guid> linkedIds = await Context
            .Lyrics.ApplySpecification(
                specification: new LyricsByStatusSpecification(status: EnumContentStatus.Published)
            )
            .Where(lyrics => lyrics.VideoId.HasValue && videoIds.Contains(lyrics.VideoId.Value))
            .Select(lyrics => lyrics.VideoId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return linkedIds.ToHashSet();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, VideoEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    )
    {
        List<VideoEntity> entities = await Context
            .Videos.Where(entity => ids.Contains(entity.Id))
            .ToListAsync(cancellationToken);

        return entities.ToDictionary(entity => entity.Id);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, VideoEntity>> GetPublishedByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    )
    {
        List<VideoEntity> entities = await Context
            .Videos.ApplySpecification(specification: new VideoByStatusSpecification(EnumContentStatus.Published))
            .Where(entity => ids.Contains(entity.Id))
            .ToListAsync(cancellationToken);

        return entities.ToDictionary(entity => entity.Id);
    }

    /// <summary>
    /// Loads the videos a page of interaction rows points at, keyed by id.
    /// </summary>
    /// <param name="videoIds">The video ids on the page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The videos by id.</returns>
    private async Task<Dictionary<Guid, VideoEntity>> LoadVideosAsync(
        Guid[] videoIds,
        CancellationToken cancellationToken
    )
    {
        if (videoIds.Length == 0)
        {
            return [];
        }

        return await Context
            .Videos.Where(video => videoIds.Contains(video.Id))
            .ToDictionaryAsync(video => video.Id, cancellationToken);
    }
}
