using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Interactions.Specifications;

/// <summary>
/// Specification that matches a playlist by its unique identifier.
/// </summary>
public class PlaylistByIdSpecification(Guid id) : Specification<PlaylistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PlaylistEntity, bool>> ToExpression()
    {
        return playlist => playlist.Id == id;
    }
}

/// <summary>
/// Specification that matches playlists belonging to a specific user.
/// </summary>
public class PlaylistByUserIdSpecification(Guid userId) : Specification<PlaylistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<PlaylistEntity, bool>> ToExpression()
    {
        return playlist => playlist.UserId == userId;
    }
}
