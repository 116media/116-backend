namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Package</c> and <c>PackageSlot</c> domains.
/// Covers conflict situations and validation failures related to package operations.
/// </summary>
public static class PackageErrorMessage
{
    /// <summary>Gets an error message for when a package name is required but not provided.</summary>
    public static string NameRequired() => "Package name is required";

    /// <summary>Gets an error message for when a package price must be zero or greater.</summary>
    public static string PriceMustBeNonNegative() => "Package price must be zero or greater";

    /// <summary>Gets an error message for when a package is already active.</summary>
    public static string AlreadyActive() => "Package is already active";

    /// <summary>Gets an error message for when a package is already inactive.</summary>
    public static string AlreadyInactive() => "Package is already inactive";

    /// <summary>Gets an error message for when a slot quantity must be greater than zero.</summary>
    public static string SlotQuantityMustBePositive() => "Slot quantity must be greater than zero";
}
