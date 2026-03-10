using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

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
/// Specification that matches a content type by its name (case-insensitive).
/// </summary>
public class ContentTypeByNameSpecification(string name) : Specification<ContentTypeEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ContentTypeEntity, bool>> ToExpression()
    {
        return contentType => EF.Functions.ILike(contentType.Name, name);
    }
}
