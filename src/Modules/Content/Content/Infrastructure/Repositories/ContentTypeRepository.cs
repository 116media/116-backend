using _116.Content.Application.Lookup.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
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
        var specification = new ContentTypeByNameSpecification(name: name);
        return await Context.ContentTypes.AnyBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContentTypeEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ContentTypeEntity> query = string.IsNullOrWhiteSpace(search)
            ? Context.ContentTypes
            : Context.ContentTypes.ApplySpecification(new ContentTypeSearchSpecification(search: search));

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContentTypeEntity>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var specification = new ActiveContentTypeSpecification();
        return await Context
            .ContentTypes.ApplySpecification(specification: specification)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
