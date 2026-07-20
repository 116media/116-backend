using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches an artist profile by its unique identifier.
/// </summary>
public class ArtistByIdSpecification(Guid id) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist => artist.Id == id;
    }
}

/// <summary>
/// Specification that matches an artist profile by its URL-safe slug (case-insensitive).
/// </summary>
public class ArtistBySlugSpecification(string slug) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist => EF.Functions.ILike(artist.Slug, slug);
    }
}

/// <summary>
/// Specification that matches the artist profile claimed by a specific identity user.
/// </summary>
public class ArtistByUserIdSpecification(Guid userId) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist => artist.UserId == userId;
    }
}

/// <summary>
/// Specification for full-text search across an artist's Name and Bio fields.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class ArtistSearchSpecification(string search) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return artist =>
            EF.Functions.ILike(artist.Name, pattern) || (artist.Bio != null && EF.Functions.ILike(artist.Bio, pattern));
    }
}
