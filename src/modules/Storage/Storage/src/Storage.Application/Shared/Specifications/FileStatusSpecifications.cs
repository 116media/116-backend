using System.Linq.Expressions;
using _116.BuildingBlocks.Domain.Specifications;
using _116.Storage.Domain.Entities;
using _116.Storage.Domain.Enums;

namespace _116.Storage.Application.Shared.Specifications;

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
