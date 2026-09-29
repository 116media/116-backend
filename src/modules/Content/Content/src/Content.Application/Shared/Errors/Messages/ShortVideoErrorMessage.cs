using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>ShortVideo</c> domain.
/// Covers conflict situations and validation failures related to short video operations.
/// </summary>
public class ShortVideoErrorMessage(IStringLocalizer<ShortVideoErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a short video title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the short video title is required.
    /// </returns>
    public string TitleRequired()
    {
        return localizer["TitleRequired"];
    }

    /// <summary>
    /// Gets an error message for when a short video is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the short video is already active.
    /// </returns>
    public string AlreadyActive()
    {
        return localizer["AlreadyActive"];
    }

    /// <summary>
    /// Gets an error message for when a short video is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the short video is already inactive.
    /// </returns>
    public string AlreadyInactive()
    {
        return localizer["AlreadyInactive"];
    }

    /// <summary>
    /// Gets an error message for when a short video with the given slug already exists.
    /// </summary>
    /// <param name="slug">The short video slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a short video with the specified slug already exists.
    /// </returns>
    public string SlugAlreadyExists(string slug)
    {
        return string.Format(localizer["SlugAlreadyExists"], slug);
    }

    /// <summary>
    /// Gets an error message for when a short video slug is required.
    /// </summary>
    public string SlugRequired() => localizer["SlugRequired"];

    /// <summary>
    /// Gets an error message for when a short video title exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string TitleTooLong(int max) => string.Format(localizer["TitleTooLong"], max);

    /// <summary>
    /// Gets an error message for when a short video slug exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SlugTooLong(int max) => string.Format(localizer["SlugTooLong"], max);

    /// <summary>
    /// Gets an error message for when a short video slug has an invalid format.
    /// </summary>
    public string SlugInvalidFormat() => localizer["SlugInvalidFormat"];

    /// <summary>
    /// Gets an error message for when a short video file is required.
    /// </summary>
    public string FileRequired() => localizer["FileRequired"];

    /// <summary>
    /// Gets an error message for when a short video file is empty.
    /// </summary>
    public string FileEmpty() => localizer["FileEmpty"];

    /// <summary>
    /// Gets an error message for when a short video file exceeds the size limit.
    /// </summary>
    /// <param name="maxMb">The maximum file size in megabytes.</param>
    public string FileTooLarge(long maxMb) => string.Format(localizer["FileTooLarge"], maxMb);

    /// <summary>
    /// Gets an error message for when a short video file has an invalid extension.
    /// </summary>
    /// <param name="allowed">Comma-separated list of allowed extensions.</param>
    public string FileInvalidExtension(string allowed) => string.Format(localizer["FileInvalidExtension"], allowed);

    /// <summary>
    /// Gets an error message for when a short video is activated before its video file is uploaded.
    /// </summary>
    public string VideoFileRequired() => localizer["VideoFileRequired"];
}
