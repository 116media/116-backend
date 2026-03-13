using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches a short video by its unique identifier.
/// </summary>
public class ShortVideoByIdSpecification(Guid id) : Specification<ShortVideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ShortVideoEntity, bool>> ToExpression()
    {
        return shortVideo => shortVideo.Id == id;
    }
}

/// <summary>
/// Specification that matches only active short videos.
/// </summary>
public class ActiveShortVideoSpecification : Specification<ShortVideoEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ShortVideoEntity, bool>> ToExpression()
    {
        return shortVideo => shortVideo.IsActive;
    }
}
