using System.Linq.Expressions;

using _116.Core.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Core.Application.Shared.Specifications;

/// <summary>
/// Specification that matches files by their unique identifier.
/// Used for direct file lookup operations.
/// </summary>
public class FileByIdSpecification(Guid fileId) : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        return file => file.Id == fileId;
    }
}

/// <summary>
/// Specification that matches files by their original filename.
/// Performs exact string comparison for filename lookup operations.
/// </summary>
public class FileByOriginalFileNameSpecification(string fileName) : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        return file => file.OriginalFileName == fileName;
    }
}

/// <summary>
/// Specification that matches files by their stored filename.
/// Used for internal file system operations and storage management.
/// </summary>
public class FileByFileNameSpecification(string fileName) : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        return file => file.FileName == fileName;
    }
}

/// <summary>
/// Specification that matches files by their MIME type.
/// Useful for filtering files by content type (e.g., images, documents, videos).
/// </summary>
public class FileByMimeTypeSpecification(string mimeType) : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        return file => file.MimeType == mimeType;
    }
}

/// <summary>
/// Composite specification that matches a non-deleted file by its ID.
/// Combines file ID lookup with soft-delete filtering, the most common file retrieval pattern.
/// </summary>
public class FileByIdNotDeletedSpecification(Guid fileId) : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        var fileByIdSpec = new FileByIdSpecification(fileId);
        var notDeletedSpec = new FileIsNotDeletedSpecification();

        return fileByIdSpec.And(notDeletedSpec).ToExpression();
    }
}
