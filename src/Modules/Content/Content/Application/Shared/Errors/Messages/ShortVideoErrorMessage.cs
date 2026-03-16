namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>ShortVideo</c> domain.
/// Covers conflict situations and validation failures related to short video operations.
/// </summary>
public static class ShortVideoErrorMessage
{
    /// <summary>
    /// Gets an error message for when a short video title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the short video title is required.
    /// </returns>
    public static string TitleRequired()
    {
        return "Short video title is required";
    }

    /// <summary>
    /// Gets an error message for when a short video is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the short video is already active.
    /// </returns>
    public static string AlreadyActive()
    {
        return "Short video is already active";
    }

    /// <summary>
    /// Gets an error message for when a short video is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the short video is already inactive.
    /// </returns>
    public static string AlreadyInactive()
    {
        return "Short video is already inactive";
    }

    /// <summary>
    /// Gets an error message for when a short video with the given slug already exists.
    /// </summary>
    /// <param name="slug">The short video slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a short video with the specified slug already exists.
    /// </returns>
    public static string SlugAlreadyExists(string slug)
    {
        return $"Short video with slug '{slug}' already exists";
    }
}
