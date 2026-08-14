using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
/// Specification that matches albums linked to a specific artist profile. Composed with
/// <see cref="AlbumByReleaseTypeSpecification" /> by the artist-scoped release query, and
/// used alone by the artist content predicate, which does not filter by type.
/// </summary>
public class AlbumByArtistSpecification(Guid artistId) : Specification<AlbumEntity>
{
    /// <inheritdoc />
    public override Expression<Func<AlbumEntity, bool>> ToExpression()
    {
        return album => album.ArtistId == artistId;
    }
}

/// <summary>
/// Specification that matches albums of a specific release type.
/// </summary>
public class AlbumByReleaseTypeSpecification(EnumReleaseType releaseType) : Specification<AlbumEntity>
{
    /// <inheritdoc />
    public override Expression<Func<AlbumEntity, bool>> ToExpression()
    {
        return album => album.ReleaseType == releaseType;
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
