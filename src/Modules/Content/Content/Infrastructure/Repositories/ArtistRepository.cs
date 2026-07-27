using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IArtistRepository" /> for managing artist profile entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ArtistRepository(ContentDbContext context) : IArtistRepository
{
    /// <inheritdoc />
    public async Task<ArtistEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new ArtistBySlugSpecification(slug: slug);
        return await context.Artists.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<ArtistEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new ArtistByIdSpecification(id: id);
        return await context.Artists.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<ArtistEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new ArtistByIdSpecification(id: id);
        return await context
            .Artists.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArtistEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var specification = new ArtistByUserIdSpecification(userId: userId);
        return await context.Artists.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<(List<ArtistEntity> Artists, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ArtistEntity> query = context.Artists;

        if (!string.IsNullOrWhiteSpace(search))
        {
            Specification<ArtistEntity> spec = new ArtistSearchSpecification(search: search);
            query = query.ApplySpecification(specification: spec);
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
    public async Task AddAsync(ArtistEntity artist, CancellationToken cancellationToken = default)
    {
        await context.Artists.AddAsync(artist, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(ArtistEntity artist)
    {
        context.Artists.Update(artist);
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
        IQueryable<ArtistEntity> query = context.Artists.ApplySpecification(
            specification: new ArtistHasContentSpecification(
                lyrics: context.Lyrics,
                videos: context.Videos,
                albums: context.Albums,
                articleArtists: context.ArticleArtists
            )
        );

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
                context.Lyrics.Count(l => l.ArtistId == a.Id && l.Status == EnumContentStatus.Published)
                    + context.Videos.Count(v => v.ArtistId == a.Id && v.Status == EnumContentStatus.Published)
                    + context.Albums.Count(al =>
                        al.ArtistId == a.Id
                        && (al.ReleaseType == EnumReleaseType.Album || al.ReleaseType == EnumReleaseType.Mixtape)
                    )
                    + context.ArticleArtists.Count(aa =>
                        aa.ArtistId == a.Id && aa.Article.Status == EnumContentStatus.Published
                    )
            ))
            .ToListAsync(cancellationToken: cancellationToken);

        return (artists, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetAvailableLettersAsync(CancellationToken cancellationToken = default)
    {
        return await context
            .Artists.ApplySpecification(
                specification: new ArtistHasContentSpecification(
                    lyrics: context.Lyrics,
                    videos: context.Videos,
                    albums: context.Albums,
                    articleArtists: context.ArticleArtists
                )
            )
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
        ArtistTotals? totals = await context
            .Artists.Where(a => a.Id == artistId)
            .Select(a => new ArtistTotals(
                context.Lyrics.Count(l => l.ArtistId == a.Id && l.Status == EnumContentStatus.Published),
                context.Videos.Count(v => v.ArtistId == a.Id && v.Status == EnumContentStatus.Published),
                context.Albums.Count(al => al.ArtistId == a.Id && al.ReleaseType == EnumReleaseType.Album),
                context.Albums.Count(al => al.ArtistId == a.Id && al.ReleaseType == EnumReleaseType.Mixtape),
                context.ArticleArtists.Count(aa =>
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
        return await context
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
        return await context.ArtistSocialLinks.FirstOrDefaultAsync(
            link => link.ArtistId == artistId && link.Platform == platform,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddSocialLinkAsync(ArtistSocialLinkEntity link, CancellationToken cancellationToken = default)
    {
        await context.ArtistSocialLinks.AddAsync(link, cancellationToken);
    }

    /// <inheritdoc />
    public void UpdateSocialLink(ArtistSocialLinkEntity link)
    {
        context.ArtistSocialLinks.Update(link);
    }

    /// <inheritdoc />
    public void RemoveSocialLink(ArtistSocialLinkEntity link)
    {
        context.ArtistSocialLinks.Remove(link);
    }
}
