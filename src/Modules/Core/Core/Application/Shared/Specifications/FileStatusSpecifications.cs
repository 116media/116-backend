using System.Linq.Expressions;

using _116.Core.Domain.Entities;
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
        return file => !file.IsDeleted;
    }
}

/// <summary>
/// Specification that matches files that have been soft-deleted.
/// Useful for recovery operations or cleanup processes.
/// </summary>
public class FileIsDeletedSpecification : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        return file => file.IsDeleted;
    }
}

/// <summary>
/// Specification that matches files within a specific size range.
/// Useful for filtering files by storage requirements or upload limits.
/// </summary>
public class FileBySizeRangeSpecification(long minSize, long maxSize) : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        return file => file.SizeInBytes >= minSize && file.SizeInBytes <= maxSize;
    }
}
