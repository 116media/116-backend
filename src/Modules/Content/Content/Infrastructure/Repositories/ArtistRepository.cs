using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IArtistRepository" /> for managing artist profile entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ArtistRepository(ContentDbContext context) : ContentRepository<ArtistEntity>(context), IArtistRepository
{
    /// <inheritdoc />
    public async Task<ArtistEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Context.Artists.FirstOrDefaultAsync(
            artist => EF.Functions.ILike(artist.Slug, slug),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<ArtistEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Artists.FirstOrDefaultAsync(artist => artist.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<ArtistEntity> Artists, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ArtistEntity> query = Context.Artists;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(artist =>
                EF.Functions.ILike(artist.Name, pattern)
                || (artist.Bio != null && EF.Functions.ILike(artist.Bio, pattern))
            );
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<ArtistEntity> artists = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (artists, totalCount);
    }

    /// <inheritdoc />
    public async Task<(List<ArtistDirectoryRow> Artists, int TotalCount)> GetPublicDirectoryAsync(
        int page,
        int pageSize,
        string? letter,
        string? search,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ArtistEntity> query = ArtistsWithPublicContent();

        if (!string.IsNullOrWhiteSpace(value: letter))
        {
            query = query.Where(a => a.InitialLetter == letter);
        }
        else if (!string.IsNullOrWhiteSpace(value: search))
        {
            // Both sides of the comparison are pre-folded uppercase, so a plain LIKE is
            // correct and index-friendly; ILIKE would re-do work the stored column already did.
            string pattern = $"%{ArtistEntity.FoldName(name: search)}%";
            query = query.Where(a => EF.Functions.Like(a.NameFolded, pattern));
        }

        int totalCount = await query.CountAsync(cancellationToken: cancellationToken);

        // The count is part of the same projection, so the filter, the ordering and the
        // per-row count translate to one statement with correlated subqueries — never one
        // query per row.
        List<ArtistDirectoryRow> artists = await query
            .OrderBy(a => a.NameFolded)
            .ThenBy(a => a.Id)
            .Skip(count: (page - 1) * pageSize)
            .Take(count: pageSize)
            .Select(a => new ArtistDirectoryRow(
                a,
                Context.Lyrics.Count(l => l.ArtistId == a.Id && l.Status == EnumContentStatus.Published)
                    + Context.Videos.Count(v => v.ArtistId == a.Id && v.Status == EnumContentStatus.Published)
                    + Context.Albums.Count(al =>
                        al.ArtistId == a.Id
                        && (al.ReleaseType == EnumReleaseType.Album || al.ReleaseType == EnumReleaseType.Mixtape)
                    )
                    + Context.ArticleArtists.Count(aa =>
                        aa.ArtistId == a.Id && aa.Article.Status == EnumContentStatus.Published
                    )
            ))
            .ToListAsync(cancellationToken: cancellationToken);

        return (artists, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetAvailableLettersAsync(CancellationToken cancellationToken = default)
    {
        return await ArtistsWithPublicContent()
            .Select(a => a.InitialLetter)
            .Distinct()
            .OrderBy(letter => letter)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArtistTotals> GetTotalsAsync(Guid artistId, CancellationToken cancellationToken = default)
    {
        // One statement projecting all five counts, term-for-term aligned with the
        // directory's content predicate — the profile's 404 rule sums these.
        ArtistTotals? totals = await Context
            .Artists.Where(a => a.Id == artistId)
            .Select(a => new ArtistTotals(
                Context.Lyrics.Count(l => l.ArtistId == a.Id && l.Status == EnumContentStatus.Published),
                Context.Videos.Count(v => v.ArtistId == a.Id && v.Status == EnumContentStatus.Published),
                Context.Albums.Count(al => al.ArtistId == a.Id && al.ReleaseType == EnumReleaseType.Album),
                Context.Albums.Count(al => al.ArtistId == a.Id && al.ReleaseType == EnumReleaseType.Mixtape),
                Context.ArticleArtists.Count(aa =>
                    aa.ArtistId == a.Id && aa.Article.Status == EnumContentStatus.Published
                )
            ))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return totals ?? new ArtistTotals(Songs: 0, Videos: 0, Albums: 0, Mixtapes: 0, News: 0);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArtistSocialLinkEntity>> GetSocialLinksAsync(
        Guid artistId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .ArtistSocialLinks.Where(link => link.ArtistId == artistId)
            .OrderBy(link => link.Platform)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArtistSocialLinkEntity?> GetSocialLinkAsync(
        Guid artistId,
        EnumSocialPlatform platform,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .ArtistSocialLinks.AsTracking()
            .FirstOrDefaultAsync(
                link => link.ArtistId == artistId && link.Platform == platform,
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task AddSocialLinkAsync(ArtistSocialLinkEntity link, CancellationToken cancellationToken = default)
    {
        await Context.ArtistSocialLinks.AddAsync(link, cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveSocialLink(ArtistSocialLinkEntity link)
    {
        Context.ArtistSocialLinks.Remove(link);
    }

    /// <summary>
    /// Artists with at least one publicly visible piece of content — a published lyrics page,
    /// video or tagged article, or a full-length album/mixtape. Term-for-term aligned with the
    /// per-row counts in <see cref="GetPublicDirectoryAsync" /> and <see cref="GetTotalsAsync" />.
    /// </summary>
    /// <returns>The filtered artist query.</returns>
    private IQueryable<ArtistEntity> ArtistsWithPublicContent()
    {
        return Context.Artists.Where(artist =>
            Context.Lyrics.Any(l => l.ArtistId == artist.Id && l.Status == EnumContentStatus.Published)
            || Context.Videos.Any(v => v.ArtistId == artist.Id && v.Status == EnumContentStatus.Published)
            || Context.Albums.Any(a =>
                a.ArtistId == artist.Id
                && (a.ReleaseType == EnumReleaseType.Album || a.ReleaseType == EnumReleaseType.Mixtape)
            )
            || Context.ArticleArtists.Any(aa =>
                aa.ArtistId == artist.Id && aa.Article.Status == EnumContentStatus.Published
            )
        );
    }
}
