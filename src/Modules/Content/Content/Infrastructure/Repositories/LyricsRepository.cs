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
/// Implementation of <see cref="ILyricsRepository" /> for managing lyrics entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class LyricsRepository(ContentDbContext context) : ContentRepository<LyricsEntity>(context), ILyricsRepository
{
    /// <inheritdoc />
    public async Task<(List<LyricsEntity> Lyrics, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        EnumContentStatus? status,
        Guid? categoryId,
        string? language = null,
        string? sort = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<LyricsEntity> query = Context.Lyrics;

        Specification<LyricsEntity>? spec = new LyricsQueryBuilder()
            .WithSearch(search: search)
            .WithStatus(status: status)
            .WithCategory(categoryId: categoryId)
            .WithLanguage(language: language)
            .Build();

        if (spec is not null)
        {
            query = query.ApplySpecification(specification: spec);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        // "newest" is both the explicit sort value and the implicit default. IsPromoted is
        // deliberately never a branch here — promoted placement renders in its own separate,
        // visually distinct slot the frontend composes independently, never by reordering this
        // organic ranking (spec 12/13). Do not add an IsPromoted-aware case.
        IOrderedQueryable<LyricsEntity> sortedQuery = sort switch
        {
            "views" => query.OrderByDescending(l => l.ViewCount).ThenByDescending(l => l.CreatedAt),
            "likes" => query.OrderByDescending(l => l.LikeCount).ThenByDescending(l => l.CreatedAt),
            "shares" => query.OrderByDescending(l => l.ShareCount).ThenByDescending(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.CreatedAt),
        };

        List<LyricsEntity> lyrics = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (lyrics, totalCount);
    }

    /// <inheritdoc />
    public async Task<LyricsEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsBySlugSpecification(slug: slug);
        return await Context
            .Lyrics.ApplySpecification(specification: specification)
            .Include(l => l.Tags)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override IQueryable<LyricsEntity> Query()
    {
        return Context.Lyrics.Include(l => l.Tags);
    }

    /// <inheritdoc />
    public async Task<LyricsEntity?> GetByVideoIdAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsByVideoIdSpecification(videoId: videoId);
        return await Context
            .Lyrics.ApplySpecification(specification: specification)
            .Include(l => l.Tags)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<LyricsEntity> Lyrics, int TotalCount)> GetPublishedByArtistAsync(
        Guid artistId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        Specification<LyricsEntity> specification = new LyricsByStatusSpecification(EnumContentStatus.Published).And(
            new LyricsByArtistSpecification(artistId: artistId)
        );

        IQueryable<LyricsEntity> query = Context.Lyrics.ApplySpecification(specification: specification);

        int totalCount = await query.CountAsync(cancellationToken);

        List<LyricsEntity> lyrics = await query
            .OrderByDescending(l => l.PublishedAt)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (lyrics, totalCount);
    }

    /// <inheritdoc />
    public async Task<List<LyricsEntity>> GetPublishedByAlbumAsync(
        Guid albumId,
        Guid excludeLyricsId,
        CancellationToken cancellationToken = default
    )
    {
        Specification<LyricsEntity> specification = new LyricsByAlbumSpecification(albumId: albumId).And(
            new LyricsByStatusSpecification(EnumContentStatus.Published)
        );

        return await Context
            .Lyrics.ApplySpecification(specification: specification)
            .Where(l => l.Id != excludeLyricsId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LyricsEntity?> GetByOrderItemIdAsync(
        Guid orderItemId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new LyricsByOrderItemIdSpecification(orderItemId: orderItemId);
        return await Context
            .Lyrics.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasLikedAsync(Guid userId, Guid lyricsId, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsLikeByUserAndLyricsSpecification(userId: userId, lyricsId: lyricsId);
        return await Context.LyricsLikes.ApplySpecification(specification: specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddLikeAsync(LyricsLikeEntity like, CancellationToken cancellationToken = default)
    {
        await Context.LyricsLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveLikeAsync(Guid userId, Guid lyricsId, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsLikeByUserAndLyricsSpecification(userId: userId, lyricsId: lyricsId);
        LyricsLikeEntity? like = await Context
            .LyricsLikes.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (like is not null)
        {
            like.MarkRemoved();
            Context.LyricsLikes.Remove(like);
        }
    }

    /// <inheritdoc />
    public async Task AddShareAsync(LyricsShareEntity share, CancellationToken cancellationToken = default)
    {
        await Context.LyricsShares.AddAsync(share, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddViewEventAsync(LyricsViewEventEntity viewEvent, CancellationToken cancellationToken = default)
    {
        await Context.LyricsViewEvents.AddAsync(viewEvent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasCountedViewSinceAsync(
        Guid lyricsId,
        string dedupKey,
        DateTime since,
        CancellationToken cancellationToken = default
    )
    {
        return await Context.LyricsViewEvents.AnyBySpecificationAsync(
            specification: new LyricsCountedViewSinceSpecification(
                lyricsId: lyricsId,
                dedupKey: dedupKey,
                since: since
            ),
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlySet<Guid>> GetLikedIdsAsync(
        Guid? currentUserId,
        IReadOnlyCollection<Guid> lyricsIds,
        CancellationToken cancellationToken = default
    )
    {
        if (currentUserId is not Guid userId || lyricsIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        List<Guid> likedIds = await Context
            .LyricsLikes.ApplySpecification(specification: new LyricsLikeByUserIdSpecification(userId: userId))
            .Where(like => lyricsIds.Contains(like.LyricsId))
            .Select(like => like.LyricsId)
            .ToListAsync(cancellationToken);

        return likedIds.ToHashSet();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LyricsEntity>> GetSimilarAsync(
        Guid lyricsId,
        CancellationToken cancellationToken = default
    )
    {
        LyricsEntity lyrics = await GetByIdOrThrowAsync(id: lyricsId, cancellationToken: cancellationToken);

        if (lyrics.VideoId is Guid videoId)
        {
            Guid? categoryId = await Context
                .Videos.ApplySpecification(specification: new VideoByIdSpecification(id: videoId))
                .Select(v => (Guid?)v.CategoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (categoryId is Guid resolvedCategoryId)
            {
                var categorySpecification = new LyricsSimilarByVideoCategorySpecification(
                    categoryId: resolvedCategoryId,
                    excludeId: lyricsId,
                    Context.Videos
                );

                List<LyricsEntity> categoryMatches = await Context
                    .Lyrics.ApplySpecification(specification: categorySpecification)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(10)
                    .ToListAsync(cancellationToken);

                if (categoryMatches.Count > 0)
                {
                    return categoryMatches;
                }
            }
        }

        List<Guid> tagIds = lyrics.Tags.Select(t => t.TagId).ToList();

        if (tagIds.Count > 0)
        {
            var tagsSpecification = new LyricsBySharedTagsSpecification(tagIds: tagIds, excludeId: lyricsId);

            List<LyricsEntity> tagMatches = await Context
                .Lyrics.ApplySpecification(specification: tagsSpecification)
                .Select(l => new { Lyrics = l, SharedCount = l.Tags.Count(t => tagIds.Contains(t.TagId)) })
                .OrderByDescending(x => x.SharedCount)
                .ThenByDescending(x => x.Lyrics.CreatedAt)
                .Take(10)
                .Select(x => x.Lyrics)
                .ToListAsync(cancellationToken);

            if (tagMatches.Count > 0)
            {
                return tagMatches;
            }
        }

        var standaloneSpecification = new LyricsStandaloneSpecification(excludeId: lyricsId);

        return await Context
            .Lyrics.ApplySpecification(specification: standaloneSpecification)
            .OrderByDescending(l => l.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int?> ApplyEngagementDeltaAsync(
        Guid lyricsId,
        EnumEngagementKind kind,
        int delta,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<LyricsEntity> row = Context.Lyrics.ApplySpecification(
            specification: new LyricsByIdSpecification(id: lyricsId)
        );

        // Math.Max reaches PostgreSQL as GREATEST, so a racing unlike cannot go negative.
        return kind switch
        {
            EnumEngagementKind.Like => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.LikeCount, e => Math.Max(0, e.LikeCount + delta)),
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
