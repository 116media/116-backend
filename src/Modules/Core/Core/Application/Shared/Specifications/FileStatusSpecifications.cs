using System.Linq.Expressions;
using _116.Core.Domain.Entities;
using _116.Core.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Core.Application.Shared.Specifications;

/// <summary>
/// Specification that matches files that have not been soft-deleted.
/// This is the default filter for most file operations to exclude deleted files from results.
/// </summary>
public class FileIsNotDeletedSpecification : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        return file => file.State != EnumFileState.Deleted && file.State != EnumFileState.Replaced;
    }
}
