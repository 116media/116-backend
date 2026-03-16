namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Package</c> and <c>PackageSlot</c> domains.
/// Covers conflict situations and validation failures related to package operations.
/// </summary>
public static class PackageErrorMessage
{
    /// <summary>
    /// Gets an error message for when a package name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the package name is required.
    /// </returns>
    public static string NameRequired()
    {
        return "Package name is required";
    }

    /// <summary>
    /// Gets an error message for when a package price must be zero or greater.
    /// </summary>
    /// <returns>
    /// An error message indicating that the package price must be a non-negative value.
    /// </returns>
    public static string PriceMustBeNonNegative()
    {
        return "Package price must be zero or greater";
    }

    /// <summary>
    /// Gets an error message for when a package is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the package is already active.
    /// </returns>
    public static string AlreadyActive()
    {
        return "Package is already active";
    }

    /// <summary>
    /// Gets an error message for when a package is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the package is already inactive.
    /// </returns>
    public static string AlreadyInactive()
    {
        return "Package is already inactive";
    }

    /// <summary>
    /// Gets an error message for when a slot quantity must be greater than zero.
    /// </summary>
    /// <returns>
    /// An error message indicating that the slot quantity must be a positive value.
    /// </returns>
    public static string SlotQuantityMustBePositive()
    {
        return "Slot quantity must be greater than zero";
    }
}
