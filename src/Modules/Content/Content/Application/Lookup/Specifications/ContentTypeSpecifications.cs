using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Lookup.Specifications;

/// <summary>
/// Specification that matches a content type by its unique identifier.
/// </summary>
public class ContentTypeByIdSpecification(Guid id) : Specification<ContentTypeEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentTypeEntity, bool>> ToExpression()
    {
        return contentType => contentType.Id == id;
    }
}

/// <summary>
/// Specification that matches a content type by its name.
/// </summary>
public class ContentTypeByNameSpecification(string name) : Specification<ContentTypeEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentTypeEntity, bool>> ToExpression()
    {
        return contentType => contentType.Name == name;
    }
}
