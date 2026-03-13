namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Article</c> domain.
/// Covers conflict situations and validation failures related to article operations.
/// </summary>
public static class ArticleErrorMessage
{
    /// <summary>
    /// Gets an error message for when an article with the given slug already exists.
    /// </summary>
    /// <param name="slug">The article slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that an article with the specified slug already exists.
    /// </returns>
    public static string SlugAlreadyExists(string slug)
    {
        return $"Article with slug '{slug}' already exists";
    }

    /// <summary>
    /// Gets an error message for when an article title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the article title is required.
    /// </returns>
    public static string TitleRequired()
    {
        return "Article title is required";
    }

    /// <summary>
    /// Gets an error message for when an article slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the article slug is required.
    /// </returns>
    public static string SlugRequired()
    {
        return "Article slug is required";
    }

    /// <summary>
    /// Gets an error message for when an invalid status transition is attempted.
    /// </summary>
    /// <param name="from">The current status of the article.</param>
    /// <param name="to">The target status that the transition was attempted towards.</param>
    /// <returns>
    /// A formatted error message indicating that the transition from the current status to the target status is not allowed.
    /// </returns>
    public static string InvalidStatusTransition(string from, string to)
    {
        return $"Cannot transition article from '{from}' to '{to}'";
    }

    /// <summary>
    /// Gets an error message for when a published or approved article is hard-deleted.
    /// </summary>
    /// <returns>
    /// An error message indicating that only articles in Draft or Rejected status can be permanently deleted.
    /// </returns>
    public static string CannotDeletePublishedArticle()
    {
        return "Only articles in Draft or Rejected status can be permanently deleted. Archive published articles instead.";
    }
}
