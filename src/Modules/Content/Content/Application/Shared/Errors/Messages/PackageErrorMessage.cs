using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Package</c> and <c>PackageSlot</c> domains.
/// Covers conflict situations and validation failures related to package operations.
/// </summary>
public class PackageErrorMessage(IStringLocalizer<PackageErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when a package name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the package name is required.
    /// </returns>
    public string NameRequired()
    {
        return localizer["NameRequired"];
    }

    /// <summary>
    /// Gets an error message for when a package price must be zero or greater.
    /// </summary>
    /// <returns>
    /// An error message indicating that the package price must be a non-negative value.
    /// </returns>
    public string PriceMustBeNonNegative()
    {
        return localizer["PriceMustBeNonNegative"];
    }

    /// <summary>
    /// Gets an error message for when a package is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the package is already active.
    /// </returns>
    public string AlreadyActive()
    {
        return localizer["AlreadyActive"];
    }

    /// <summary>
    /// Gets an error message for when a package is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the package is already inactive.
    /// </returns>
    public string AlreadyInactive()
    {
        return localizer["AlreadyInactive"];
    }

    /// <summary>
    /// Gets an error message for when a slot quantity must be greater than zero.
    /// </summary>
    /// <returns>
    /// An error message indicating that the slot quantity must be a positive value.
    /// </returns>
    public string SlotQuantityMustBePositive()
    {
        return localizer["SlotQuantityMustBePositive"];
    }

    /// <summary>
    /// Gets an error message for when a package name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string NameTooLong(int max) => string.Format(localizer["NameTooLong"], max);

    /// <summary>
    /// Gets an error message for when a package description is required.
    /// </summary>
    public string DescriptionRequired() => localizer["DescriptionRequired"];

    /// <summary>
    /// Gets an error message for when a package description exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string DescriptionTooLong(int max) => string.Format(localizer["DescriptionTooLong"], max);
}
