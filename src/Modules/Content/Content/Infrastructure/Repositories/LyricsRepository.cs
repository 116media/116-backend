using _116.Content.Application.Editorial.Builders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
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
        IQueryable<LyricsEntity> query = Context.Lyrics.Include(l => l.Category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(lyrics =>
                EF.Functions.ILike(lyrics.SongTitle, pattern)
                || EF.Functions.ILike(lyrics.ArtistName, pattern)
                || EF.Functions.ILike(lyrics.LyricsText, pattern)
            );
        }

        if (status.HasValue)
        {
            query = query.Where(lyrics => lyrics.Status == status.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(lyrics => lyrics.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(lyrics => EF.Functions.ILike(lyrics.Language, language));
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
        return await Context
            .Lyrics.Where(lyrics => EF.Functions.ILike(lyrics.Slug, slug))
            .Include(l => l.Category)
            .Include(l => l.Customer)
            .Include(l => l.Tags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<LyricsEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context
            .Lyrics.Where(lyrics => lyrics.Id == id)
            .Include(l => l.Category)
            .Include(l => l.Customer)
            .Include(l => l.Tags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<LyricsEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context
            .Lyrics.AsTracking()
            .Where(lyrics => lyrics.Id == id)
            .Include(l => l.Category)
            .Include(l => l.Customer)
            .Include(l => l.Tags)
                .ThenInclude(t => t.Tag)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LyricsEntity?> GetByVideoIdAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        return await Context
            .Lyrics.Where(lyrics => lyrics.VideoId == videoId)
            .Include(l => l.Category)
            .Include(l => l.Customer)
            .Include(l => l.Tags)
                .ThenInclude(t => t.Tag)
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
        IQueryable<LyricsEntity> query = Context
            .Lyrics.Include(l => l.Category)
            .Where(lyrics => lyrics.Status == EnumContentStatus.Published && lyrics.ArtistId == artistId);

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
        return await Context
            .Lyrics.Where(lyrics => lyrics.AlbumId == albumId && lyrics.Status == EnumContentStatus.Published)
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
        return await Context
            .Lyrics.AsTracking()
            .FirstOrDefaultAsync(lyrics => lyrics.OrderItemId == orderItemId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReplaceTagsAsync(
        Guid lyricsId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken = default
    )
    {
        List<LyricsTagEntity> existingTags = await Context
            .LyricsTags.AsTracking()
            .Where(t => t.LyricsId == lyricsId)
            .ToListAsync(cancellationToken);

        foreach (LyricsTagEntity existingTag in existingTags)
        {
            existingTag.MarkRemoved();
        }

        Context.LyricsTags.RemoveRange(existingTags);

        foreach (Guid tagId in tagIds)
        {
            await Context.LyricsTags.AddAsync(
                LyricsTagEntity.Create(id: Guid.NewGuid(), lyricsId: lyricsId, tagId: tagId),
                cancellationToken
            );
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasLikedAsync(Guid userId, Guid lyricsId, CancellationToken cancellationToken = default)
    {
        return await Context.LyricsLikes.AnyAsync(
            like => like.UserId == userId && like.LyricsId == lyricsId,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddLikeAsync(LyricsLikeEntity like, CancellationToken cancellationToken = default)
    {
        await Context.LyricsLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveLikeAsync(Guid userId, Guid lyricsId, CancellationToken cancellationToken = default)
    {
        LyricsLikeEntity? like = await Context
            .LyricsLikes.AsTracking()
            .FirstOrDefaultAsync(l => l.UserId == userId && l.LyricsId == lyricsId, cancellationToken);

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
        return await Context.LyricsViewEvents.AnyAsync(
            x => x.LyricsId == lyricsId && x.DedupKey == dedupKey && x.IsCounted && x.CreatedAt >= since,
            cancellationToken
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
            .LyricsLikes.Where(like => like.UserId == userId && lyricsIds.Contains(like.LyricsId))
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
                .Videos.Where(v => v.Id == videoId)
                .Select(v => (Guid?)v.CategoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (categoryId is Guid resolvedCategoryId)
            {
                List<LyricsEntity> categoryMatches = await Context
                    .Lyrics.Include(l => l.Category)
                    .Where(similar =>
                        similar.Id != lyricsId
                        && similar.Status == EnumContentStatus.Published
                        && similar.Video != null
                        && similar.Video.CategoryId == resolvedCategoryId
                    )
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
            List<LyricsEntity> tagMatches = await Context
                .Lyrics.Include(l => l.Category)
                .Where(similar =>
                    similar.Id != lyricsId
                    && similar.Status == EnumContentStatus.Published
                    && similar.Tags.Any(t => tagIds.Contains(t.TagId))
                )
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

        return await Context
            .Lyrics.Include(l => l.Category)
            .Where(similar =>
                similar.Id != lyricsId && similar.Status == EnumContentStatus.Published && similar.VideoId == null
            )
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
        IQueryable<LyricsEntity> row = Context.Lyrics.Where(e => e.Id == lyricsId);

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
