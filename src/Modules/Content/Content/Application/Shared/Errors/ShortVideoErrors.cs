using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// ShortVideo domain error factory providing simple, readable exception creation.
/// Usage: ShortVideoErrors.NotFound(id) or ShortVideoErrors.AlreadyActive()
/// </summary>
public class ShortVideoErrors(ShortVideoErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public ShortVideoErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a short video is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("ShortVideo", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a short video is already active.
    /// </summary>
    public ConflictException AlreadyActive()
    {
        return new ConflictException(i18n.AlreadyActive());
    }

    /// <summary>
    /// Throws when a short video is already inactive.
    /// </summary>
    public ConflictException AlreadyInactive()
    {
        return new ConflictException(i18n.AlreadyInactive());
    }

    /// <summary>
    /// Throws when a short video with the given slug already exists.
    /// </summary>
    public ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(i18n.SlugAlreadyExists(slug: slug));
    }
}
