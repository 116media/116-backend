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
public class ArtistRepository(ContentDbContext context) : ContentRepository<ArtistEntity>(context), IArtistRepository
{
    /// <inheritdoc />
    public async Task<ArtistEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new ArtistBySlugSpecification(slug: slug);
        return await Query()
            .FirstOrDefaultBySpecificationAsync(specification: specification, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArtistEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var specification = new ArtistByUserIdSpecification(userId: userId);
        return await Context.Artists.FirstOrDefaultBySpecificationAsync(
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
        IQueryable<ArtistEntity> query = Context.Artists;

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
    public async Task<(List<ArtistDirectoryRow> Artists, int TotalCount)> GetPublicDirectoryAsync(
        int page,
        int pageSize,
        string? letter,
        string? search,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ArtistEntity> query = Context.Artists.ApplySpecification(
            specification: new ArtistHasContentSpecification(
                lyrics: Context.Lyrics,
                videos: Context.Videos,
                albums: Context.Albums,
                articleArtists: Context.ArticleArtists,
                articles: Context.Articles
            )
        );

        if (!string.IsNullOrWhiteSpace(value: letter))
        {
            query = query.ApplySpecification(specification: new ArtistByInitialLetterSpecification(letter: letter));
        }
        else if (!string.IsNullOrWhiteSpace(value: search))
        {
            query = query.ApplySpecification(specification: new ArtistByFoldedNameSpecification(search: search));
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
                        aa.ArtistId == a.Id
                        && Context.Articles.Any(article =>
                            article.Id == aa.ArticleId && article.Status == EnumContentStatus.Published
                        )
                    )
            ))
            .ToListAsync(cancellationToken: cancellationToken);

        return (artists, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetAvailableLettersAsync(CancellationToken cancellationToken = default)
    {
        return await Context
            .Artists.ApplySpecification(
                specification: new ArtistHasContentSpecification(
                    lyrics: Context.Lyrics,
                    videos: Context.Videos,
                    albums: Context.Albums,
                    articleArtists: Context.ArticleArtists,
                    articles: Context.Articles
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
        ArtistTotals? totals = await Context
            .Artists.ApplySpecification(specification: new ArtistByIdSpecification(id: artistId))
            .Select(a => new ArtistTotals(
                Context.Lyrics.Count(l => l.ArtistId == a.Id && l.Status == EnumContentStatus.Published),
                Context.Videos.Count(v => v.ArtistId == a.Id && v.Status == EnumContentStatus.Published),
                Context.Albums.Count(al => al.ArtistId == a.Id && al.ReleaseType == EnumReleaseType.Album),
                Context.Albums.Count(al => al.ArtistId == a.Id && al.ReleaseType == EnumReleaseType.Mixtape),
                Context.ArticleArtists.Count(aa =>
                    aa.ArtistId == a.Id
                    && Context.Articles.Any(article =>
                        article.Id == aa.ArticleId && article.Status == EnumContentStatus.Published
                    )
                )
            ))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return totals ?? new ArtistTotals(Songs: 0, Videos: 0, Albums: 0, Mixtapes: 0, News: 0);
    }

    /// <inheritdoc />
    protected override IQueryable<ArtistEntity> Query()
    {
        return Context.Artists.Include(artist => artist.SocialLinks);
    }
}
