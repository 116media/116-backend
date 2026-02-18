using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Tag domain error factory providing simple, readable exception creation.
/// Usage: TagErrors.SlugAlreadyExists(slug) or TagErrors.NotFound(id)
/// </summary>
public static class TagErrors
{
    /// <summary>
    /// Throws when a tag with the given slug already exists.
    /// </summary>
    public static ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(TagErrorMessage.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when a tag is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Tag", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a tag name is required but not provided.
    /// </summary>
    public static BadRequestException NameRequired()
    {
        return new BadRequestException(TagErrorMessage.NameRequired());
    }

    /// <summary>
    /// Throws when a tag slug is required but not provided.
    /// </summary>
    public static BadRequestException SlugRequired()
    {
        return new BadRequestException(TagErrorMessage.SlugRequired());
    }
}
