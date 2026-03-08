namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>ContentType</c> domain.
/// Covers conflict situations and validation failures related to content type operations.
/// </summary>
public static class ContentTypeErrorMessage
{
    /// <summary>
    /// Gets an error message for when a content type with the given name already exists.
    /// </summary>
    /// <param name="name">The content type name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a content type with the specified name already exists.
    /// </returns>
    public static string AlreadyExists(string name)
    {
        return $"Content type '{name}' already exists";
    }

    /// <summary>
    /// Gets an error message for when a content type is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the content type is already active.
    /// </returns>
    public static string AlreadyActive()
    {
        return "Content type is already active";
    }

    /// <summary>
    /// Gets an error message for when a content type is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the content type is already inactive.
    /// </returns>
    public static string AlreadyInactive()
    {
        return "Content type is already inactive";
    }

    /// <summary>
    /// Gets an error message for when a content type name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the content type name is required.
    /// </returns>
    public static string NameRequired()
    {
        return "Content type name is required";
    }
}
