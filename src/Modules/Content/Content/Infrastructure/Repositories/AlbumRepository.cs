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
/// Implementation of <see cref="IAlbumRepository" /> for managing album entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class AlbumRepository(ContentDbContext context) : ContentRepository<AlbumEntity>(context), IAlbumRepository
{
    /// <inheritdoc />
    public async Task<(List<AlbumEntity> Albums, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<AlbumEntity> query = Context.Albums;

        if (!string.IsNullOrWhiteSpace(search))
        {
            Specification<AlbumEntity> spec = new AlbumSearchSpecification(search: search);
            query = query.ApplySpecification(specification: spec);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<AlbumEntity> albums = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (albums, totalCount);
    }

    /// <inheritdoc />
    public async Task<(List<AlbumEntity> Albums, int TotalCount)> GetByArtistAsync(
        Guid artistId,
        EnumReleaseType releaseType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        Specification<AlbumEntity> specification = new AlbumByArtistSpecification(artistId: artistId).And(
            new AlbumByReleaseTypeSpecification(releaseType: releaseType)
        );

        IQueryable<AlbumEntity> query = Context.Albums.ApplySpecification(specification: specification);

        int totalCount = await query.CountAsync(cancellationToken: cancellationToken);

        List<AlbumEntity> albums = await query
            .OrderBy(a => a.ReleaseYear == null)
            .ThenByDescending(a => a.ReleaseYear)
            .ThenBy(a => a.Name)
            .Skip(count: (page - 1) * pageSize)
            .Take(count: pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return (albums, totalCount);
    }
}
