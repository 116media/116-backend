using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IContentTypeRepository" /> for managing content-type entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ContentTypeRepository(ContentDbContext context)
    : ContentRepository<ContentTypeEntity>(context),
        IContentTypeRepository
{
    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Context.ContentTypes.AnyAsync(
            contentType => EF.Functions.ILike(contentType.Name, name),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContentTypeEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ContentTypeEntity> query = Context.ContentTypes;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(contentType => EF.Functions.ILike(contentType.Name, pattern));
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContentTypeEntity>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Context
            .ContentTypes.Where(contentType => contentType.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
