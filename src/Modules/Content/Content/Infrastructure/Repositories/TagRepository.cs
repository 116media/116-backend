using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Builders;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ITagRepository" /> for managing tag entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class TagRepository(ContentDbContext context) : ContentRepository<TagEntity>(context), ITagRepository
{
    /// <inheritdoc />
    public async Task<TagEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Context.Tags.FirstOrDefaultAsync(tag => tag.Slug == slug, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TagEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Context.Tags.FirstOrDefaultAsync(
            t => t.Name.ToLower() == name.ToLower(),
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, TagEntity>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default
    )
    {
        List<string> loweredNames = names.Select(name => name.ToLower()).Distinct().ToList();

        List<TagEntity> tags = await Context
            .Tags.Where(tag => loweredNames.Contains(tag.Name.ToLower()))
            .ToListAsync(cancellationToken);

        return tags.GroupBy(tag => tag.Name.ToLower()).ToDictionary(group => group.Key, group => group.First());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagEntity>> GetAllAsync(
        string? search = null,
        EnumCoreContentType? contentType = null,
        int? limit = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TagEntity> query = new AllTagsQueryBuilder()
            .WithSearch(search)
            .WithContentType(contentType)
            .WithLimit(limit)
            .Build(Context);

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagEntity>> GetPopularAsync(
        int? limit,
        EnumCoreContentType? contentType = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TagEntity> query = new PopularTagsQueryBuilder()
            .WithContentType(contentType)
            .WithLimit(limit)
            .Build(Context);

        return await query.ToListAsync(cancellationToken);
    }
}
