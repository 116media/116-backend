using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Album domain error factory providing simple, readable exception creation.
/// Usage: AlbumErrors.NotFound(id) or AlbumErrors.NameRequired()
/// </summary>
public class AlbumErrors(AlbumErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public AlbumErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when an album is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Album", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when an album name is required but not provided.
    /// </summary>
    public BadRequestException NameRequired()
    {
        return new BadRequestException(i18n.NameRequired());
    }
}
