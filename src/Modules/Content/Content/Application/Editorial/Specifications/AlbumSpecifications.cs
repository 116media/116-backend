using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches an album by its unique identifier.
/// </summary>
public class AlbumByIdSpecification(Guid id) : Specification<AlbumEntity>
{
    /// <inheritdoc />
    public override Expression<Func<AlbumEntity, bool>> ToExpression()
    {
        return album => album.Id == id;
    }
}

/// <summary>
/// Specification for full-text search across an album's Name and Label fields.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class AlbumSearchSpecification(string search) : Specification<AlbumEntity>
{
    /// <inheritdoc />
    public override Expression<Func<AlbumEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return album =>
            EF.Functions.ILike(album.Name, pattern)
            || (album.Label != null && EF.Functions.ILike(album.Label, pattern));
    }
}
