using _116.Content.Application.Editorial.Builders;
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
/// Implementation of <see cref="IVideoRepository" /> for managing video entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class VideoRepository(ContentDbContext context) : IVideoRepository
{
    /// <inheritdoc />
    public async Task<(List<VideoEntity> Videos, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        EnumContentStatus? status,
        Guid? categoryId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<VideoEntity> query = context.Videos.Include(v => v.Category);

        Specification<VideoEntity>? spec = new VideoQueryBuilder()
            .WithSearch(search: search)
            .WithStatus(status: status)
            .WithCategory(categoryId: categoryId)
            .Build();

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
    public async Task<VideoEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new VideoByIdSpecification(id: id);
        return await context
            .Videos.ApplySpecification(specification: specification)
            .Include(v => v.Category)
            .Include(v => v.Tags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VideoEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new VideoByIdSpecification(id: id);
        return await context
            .Videos.ApplySpecification(specification: specification)
            .Include(v => v.Category)
            .Include(v => v.Tags)
                .ThenInclude(t => t.Tag)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VideoEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new VideoBySlugSpecification(slug: slug);
        return await context
            .Videos.ApplySpecification(specification: specification)
            .Include(v => v.Category)
            .Include(v => v.Tags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoEntity>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        var specification = new FeaturedVideoSpecification();
        return await context
            .Videos.ApplySpecification(specification: specification)
            .Include(v => v.Category)
            .OrderByDescending(v => v.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(VideoEntity video, CancellationToken cancellationToken = default)
    {
        await context.Videos.AddAsync(video, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(VideoEntity video)
    {
        context.Videos.Update(video);
    }

    /// <inheritdoc />
    public void Remove(VideoEntity video)
    {
        context.Videos.Remove(video);
    }

    /// <inheritdoc />
    public async Task AddTagAsync(VideoTagEntity tag, CancellationToken cancellationToken = default)
    {
        await context.VideoTags.AddAsync(tag, cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveTag(VideoTagEntity tag)
    {
        context.VideoTags.Remove(tag);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoTagEntity>> GetTagsByVideoIdAsync(
        Guid videoId,
        CancellationToken cancellationToken = default
    )
    {
        return await context.VideoTags.Where(t => t.VideoId == videoId).ToListAsync(cancellationToken);
    }
}
