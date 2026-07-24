using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Artist domain error factory providing simple, readable exception creation.
/// Usage: ArtistErrors.NotFound(id) or ArtistErrors.SlugAlreadyExists(slug)
/// </summary>
public class ArtistErrors(ArtistErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public ArtistErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when an artist profile is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Artist", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when an artist name is required but not provided.
    /// </summary>
    public BadRequestException NameRequired()
    {
        return new BadRequestException(i18n.NameRequired());
    }

    /// <summary>
    /// Throws when an artist slug is required but not provided.
    /// </summary>
    public BadRequestException SlugRequired()
    {
        return new BadRequestException(i18n.SlugRequired());
    }

    /// <summary>
    /// Throws when an artist profile with the given slug already exists.
    /// </summary>
    public ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(i18n.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when attempting to claim an artist profile that has already been claimed.
    /// </summary>
    public ConflictException AlreadyClaimed()
    {
        return new ConflictException(i18n.AlreadyClaimed());
    }
}
