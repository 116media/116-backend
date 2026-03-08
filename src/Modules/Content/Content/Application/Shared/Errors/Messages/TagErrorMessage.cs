namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Tag</c> domain.
/// Covers conflict situations and validation failures related to tag operations.
/// </summary>
public static class TagErrorMessage
{
    /// <summary>
    /// Gets an error message for when a tag with the given slug already exists.
    /// </summary>
    /// <param name="slug">The tag slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a tag with the specified slug already exists.
    /// </returns>
    public static string SlugAlreadyExists(string slug)
    {
        return $"Tag with slug '{slug}' already exists";
    }

    /// <summary>
    /// Gets an error message for when a tag name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the tag name is required.
    /// </returns>
    public static string NameRequired()
    {
        return "Tag name is required";
    }

    /// <summary>
    /// Gets an error message for when a tag slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the tag slug is required.
    /// </returns>
    public static string SlugRequired()
    {
        return "Tag slug is required";
    }
}
