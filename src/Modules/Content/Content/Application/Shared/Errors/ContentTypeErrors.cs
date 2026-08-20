using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// ContentType domain error factory providing simple, readable exception creation.
/// Usage: ContentTypeErrors.AlreadyExists(name) or ContentTypeErrors.NotFound(id)
/// </summary>
public class ContentTypeErrors(ContentTypeErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public ContentTypeErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a content type with the given name already exists.
    /// </summary>
    public ConflictException AlreadyExists(string name)
    {
        return new ConflictException(i18n.AlreadyExists(name: name));
    }

    /// <summary>
    /// Throws when a content type is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("ContentType", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a content type is already active.
    /// </summary>
    public ConflictException AlreadyActive()
    {
        return new ConflictException(i18n.AlreadyActive());
    }

    /// <summary>
    /// Throws when a content type is already inactive.
    /// </summary>
    public ConflictException AlreadyInactive()
    {
        return new ConflictException(i18n.AlreadyInactive());
    }
}
