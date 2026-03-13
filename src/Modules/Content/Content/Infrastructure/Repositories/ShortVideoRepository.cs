using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IShortVideoRepository" /> for managing short video entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ShortVideoRepository(ContentDbContext context) : IShortVideoRepository
{
    /// <inheritdoc />
    public async Task<(List<ShortVideoEntity> ShortVideos, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ShortVideoEntity> query = context.ShortVideos;

        if (isActive.HasValue)
        {
            Specification<ShortVideoEntity> spec = isActive.Value
                ? new ActiveShortVideoSpecification()
                : new ActiveShortVideoSpecification().Not();
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
    public async Task<ShortVideoEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoByIdSpecification(id: id);
        return await context.ShortVideos.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<ShortVideoEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new ShortVideoByIdSpecification(id: id);
        return await context
            .ShortVideos.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(ShortVideoEntity shortVideo, CancellationToken cancellationToken = default)
    {
        await context.ShortVideos.AddAsync(shortVideo, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(ShortVideoEntity shortVideo)
    {
        context.ShortVideos.Update(shortVideo);
    }

    /// <inheritdoc />
    public void Remove(ShortVideoEntity shortVideo)
    {
        context.ShortVideos.Remove(shortVideo);
    }
}
