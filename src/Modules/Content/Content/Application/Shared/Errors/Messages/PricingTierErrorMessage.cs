using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>PricingTier</c> domain.
/// Covers conflict situations and validation failures related to pricing tier operations.
/// </summary>
public class PricingTierErrorMessage(IStringLocalizer<PricingTierErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when a pricing tier with the given name already exists.
    /// </summary>
    /// <param name="name">The pricing tier name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a pricing tier with the specified name already exists.
    /// </returns>
    public string AlreadyExists(string name)
    {
        return string.Format(localizer["AlreadyExists"], name);
    }

    /// <summary>
    /// Gets an error message for when a pricing tier is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the pricing tier is already active.
    /// </returns>
    public string AlreadyActive()
    {
        return localizer["AlreadyActive"];
    }

    /// <summary>
    /// Gets an error message for when a pricing tier is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the pricing tier is already inactive.
    /// </returns>
    public string AlreadyInactive()
    {
        return localizer["AlreadyInactive"];
    }

    /// <summary>
    /// Gets an error message for when an inactive pricing tier is used in an operation that requires an active one.
    /// </summary>
    /// <returns>
    /// An error message indicating that the pricing tier is inactive and cannot be used.
    /// </returns>
    public string IsInactive()
    {
        return localizer["IsInactive"];
    }

    /// <summary>
    /// Gets an error message for when a pricing tier name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the pricing tier name is required.
    /// </returns>
    public string NameRequired()
    {
        return localizer["NameRequired"];
    }

    /// <summary>
    /// Gets an error message for when a pricing tier name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string NameTooLong(int max) => string.Format(localizer["NameTooLong"], max);

    /// <summary>
    /// Gets an error message for when a pricing tier description is required.
    /// </summary>
    public string DescriptionRequired() => localizer["DescriptionRequired"];

    /// <summary>
    /// Gets an error message for when a pricing tier description exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string DescriptionTooLong(int max) => string.Format(localizer["DescriptionTooLong"], max);
}
