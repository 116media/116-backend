using System.Linq.Expressions;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Core.Application.Shared.Specifications;

/// <summary>
/// Specification that matches image files based on common image MIME types.
/// Includes JPEG, PNG, GIF, BMP, and WebP formats.
/// </summary>
public class FileIsImageSpecification : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        return file =>
            file.MimeType.StartsWith("image/")
            && (
                file.MimeType == "image/jpeg"
                || file.MimeType == "image/png"
                || file.MimeType == "image/gif"
                || file.MimeType == "image/bmp"
                || file.MimeType == "image/webp"
            );
    }
}

/// <summary>
/// Composite specification for valid avatar files.
/// Ensures the file is not deleted and is an image, commonly used for user avatar operations.
/// </summary>
public class FileIsValidAvatarSpecification : Specification<FileEntity>
{
    public override Expression<Func<FileEntity, bool>> ToExpression()
    {
        var notDeletedSpec = new FileIsNotDeletedSpecification();
        var imageSpec = new FileIsImageSpecification();

        return notDeletedSpec.And(imageSpec).ToExpression();
    }
}
