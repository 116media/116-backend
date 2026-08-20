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
    /// Throws when an artist profile with the given slug already exists.
    /// </summary>
    public ConflictException SlugAlreadyExists(string slug)
    {
        return new ConflictException(i18n.SlugAlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when the requesting account has already filed an ownership claim for the same
    /// artist profile.
    /// </summary>
    public ConflictException ClaimRequestAlreadyExists()
    {
        return new ConflictException(i18n.ClaimRequestAlreadyExists());
    }

    /// <summary>
    /// Throws when no social link exists for the requested platform on this artist.
    /// </summary>
    public NotFoundException SocialLinkNotFound(string platform)
    {
        return new NotFoundException("ArtistSocialLink", "platform", keyValue: platform);
    }

    /// <summary>
    /// Throws when attempting to claim an artist profile that has already been claimed.
    /// </summary>
    public ConflictException AlreadyClaimed()
    {
        return new ConflictException(i18n.AlreadyClaimed());
    }
}
