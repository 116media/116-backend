using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>ContentType</c> domain.
/// Covers conflict situations and validation failures related to content type operations.
/// </summary>
public class ContentTypeErrorMessage(IStringLocalizer<ContentTypeErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a content type with the given name already exists.
    /// </summary>
    /// <param name="name">The content type name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a content type with the specified name already exists.
    /// </returns>
    public string AlreadyExists(string name)
    {
        return string.Format(localizer["AlreadyExists"], name);
    }

    /// <summary>
    /// Gets an error message for when a content type is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the content type is already active.
    /// </returns>
    public string AlreadyActive()
    {
        return localizer["AlreadyActive"];
    }

    /// <summary>
    /// Gets an error message for when a content type is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the content type is already inactive.
    /// </returns>
    public string AlreadyInactive()
    {
        return localizer["AlreadyInactive"];
    }

    /// <summary>
    /// Gets an error message for when a content type name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the content type name is required.
    /// </returns>
    public string NameRequired()
    {
        return localizer["NameRequired"];
    }

    /// <summary>
    /// Gets an error message for when a content type name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string NameTooLong(int max) => string.Format(localizer["NameTooLong"], max);
}
