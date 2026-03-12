namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Category</c> and <c>CategoryPricing</c> domains.
/// Covers conflict situations and validation failures related to category operations.
/// </summary>
public static class CategoryErrorMessage
{
    /// <summary>
    /// Gets an error message for when a category with the given slug already exists.
    /// </summary>
    /// <param name="slug">The category slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a category with the specified slug already exists.
    /// </returns>
    public static string AlreadyExists(string slug)
    {
        return $"Category with slug '{slug}' already exists";
    }

    /// <summary>
    /// Gets an error message for when a category is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the category is already active.
    /// </returns>
    public static string AlreadyActive()
    {
        return "Category is already active";
    }

    /// <summary>
    /// Gets an error message for when a category is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the category is already inactive.
    /// </returns>
    public static string AlreadyInactive()
    {
        return "Category is already inactive";
    }

    /// <summary>
    /// Gets an error message for when a category name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the category name is required.
    /// </returns>
    public static string NameRequired()
    {
        return "Category name is required";
    }

    /// <summary>
    /// Gets an error message for when a category slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the category slug is required.
    /// </returns>
    public static string SlugRequired()
    {
        return "Category slug is required";
    }

    /// <summary>
    /// Gets an error message for when a pricing tier is already configured for the category.
    /// </summary>
    /// <returns>
    /// An error message indicating that this pricing tier combination already exists for the category.
    /// </returns>
    public static string PricingAlreadyExists()
    {
        return "This pricing tier is already configured for the category";
    }

    /// <summary>
    /// Gets an error message for when a price must be zero or greater.
    /// </summary>
    /// <returns>
    /// An error message indicating that the price must be a non-negative value.
    /// </returns>
    public static string PriceMustBeNonNegative()
    {
        return "Price must be zero or greater";
    }
}
