namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Category</c> and <c>CategoryPricing</c> domains.
/// Covers conflict situations and validation failures related to category operations.
/// </summary>
public static class CategoryErrorMessage
{
    /// <summary>Gets an error message for when a category with the given slug already exists.</summary>
    public static string AlreadyExists(string slug) => $"Category with slug '{slug}' already exists";

    /// <summary>Gets an error message for when a category is already active.</summary>
    public static string AlreadyActive() => "Category is already active";

    /// <summary>Gets an error message for when a category is already inactive.</summary>
    public static string AlreadyInactive() => "Category is already inactive";

    /// <summary>Gets an error message for when a category name is required but not provided.</summary>
    public static string NameRequired() => "Category name is required";

    /// <summary>Gets an error message for when a category slug is required but not provided.</summary>
    public static string SlugRequired() => "Category slug is required";

    /// <summary>Gets an error message for when a category pricing tier combination already exists.</summary>
    public static string PricingAlreadyExists() => "This pricing tier is already configured for the category";

    /// <summary>Gets an error message for when a price must be zero or greater.</summary>
    public static string PriceMustBeNonNegative() => "Price must be zero or greater";
}
