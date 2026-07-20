using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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
}
