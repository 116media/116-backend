using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Tag</c> domain.
/// Covers conflict situations and validation failures related to tag operations.
/// </summary>
public class TagErrorMessage(IStringLocalizer<TagErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a tag with the given slug already exists.
    /// </summary>
    /// <param name="slug">The tag slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a tag with the specified slug already exists.
    /// </returns>
    public string SlugAlreadyExists(string slug)
    {
        return string.Format(localizer["SlugAlreadyExists"], slug);
    }

    /// <summary>
    /// Gets an error message for when a tag name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the tag name is required.
    /// </returns>
    public string NameRequired()
    {
        return localizer["NameRequired"];
    }

    /// <summary>
    /// Gets an error message for when a tag slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the tag slug is required.
    /// </returns>
    public string SlugRequired()
    {
        return localizer["SlugRequired"];
    }

    /// <summary>
    /// Gets an error message for when a tag name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string NameTooLong(int max) => string.Format(localizer["NameTooLong"], max);

    /// <summary>
    /// Gets an error message for when a tag slug exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SlugTooLong(int max) => string.Format(localizer["SlugTooLong"], max);

    /// <summary>
    /// Gets an error message for when a tag slug has an invalid format.
    /// </summary>
    public string SlugInvalidFormat() => localizer["SlugInvalidFormat"];
}
