using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Tag domain error factory providing simple, readable exception creation.
/// Usage: TagErrors.SlugAlreadyExists(slug) or TagErrors.NotFound(id)
/// </summary>
public class TagErrors(TagErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public TagErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a tag with the given slug already exists.
    /// </summary>
    public ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(i18n.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when a tag is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Tag", "id", keyValue: id);
    }
}
