using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// ContentType domain error factory providing simple, readable exception creation.
/// Usage: ContentTypeErrors.AlreadyExists(name) or ContentTypeErrors.NotFound(id)
/// </summary>
public static class ContentTypeErrors
{
    /// <summary>
    /// Throws when a content type with the given name already exists.
    /// </summary>
    public static ConflictException AlreadyExists(string name)
    {
        return new ConflictException(ContentTypeErrorMessage.AlreadyExists(name: name));
    }

    /// <summary>
    /// Throws when a content type is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("ContentType", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a content type is already active.
    /// </summary>
    public static ConflictException AlreadyActive()
    {
        return new ConflictException(ContentTypeErrorMessage.AlreadyActive());
    }

    /// <summary>
    /// Throws when a content type is already inactive.
    /// </summary>
    public static ConflictException AlreadyInactive()
    {
        return new ConflictException(ContentTypeErrorMessage.AlreadyInactive());
    }

    /// <summary>
    /// Throws when a content type name is required but not provided.
    /// </summary>
    public static BadRequestException NameRequired()
    {
        return new BadRequestException(ContentTypeErrorMessage.NameRequired());
    }
}
