using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>PromotionLevel</c> domain.
/// Covers conflict situations and validation failures related to promotion level operations.
/// </summary>
public class PromotionLevelErrorMessage(IStringLocalizer<PromotionLevelErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when a promotion level with the given name already exists.
    /// </summary>
    /// <param name="name">The promotion level name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a promotion level with the specified name already exists.
    /// </returns>
    public string AlreadyExists(string name)
    {
        return string.Format(localizer["AlreadyExists"], name);
    }

    /// <summary>
    /// Gets an error message for when a promotion level is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the promotion level is already active.
    /// </returns>
    public string AlreadyActive()
    {
        return localizer["AlreadyActive"];
    }

    /// <summary>
    /// Gets an error message for when a promotion level is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the promotion level is already inactive.
    /// </returns>
    public string AlreadyInactive()
    {
        return localizer["AlreadyInactive"];
    }

    /// <summary>
    /// Gets an error message for when a promotion level name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the promotion level name is required.
    /// </returns>
    public string NameRequired()
    {
        return localizer["NameRequired"];
    }

    /// <summary>
    /// Gets an error message for when the promotion level duration is not a positive number.
    /// </summary>
    /// <returns>
    /// An error message indicating that the duration must be greater than zero.
    /// </returns>
    public string DurationMustBePositive()
    {
        return localizer["DurationMustBePositive"];
    }

    /// <summary>
    /// Gets an error message for when the promotion level price is negative.
    /// </summary>
    /// <returns>
    /// An error message indicating that the price must be zero or greater.
    /// </returns>
    public string PriceMustBeNonNegative()
    {
        return localizer["PriceMustBeNonNegative"];
    }

    /// <summary>
    /// Gets an error message for when the spot priority is not a valid grid position.
    /// </summary>
    /// <returns>
    /// An error message indicating that spot priority must be 1, 2, or 3.
    /// </returns>
    public string InvalidSpotPriority()
    {
        return localizer["InvalidSpotPriority"];
    }

    /// <summary>
    /// Gets an error message for when a promotion level name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string NameTooLong(int max) => string.Format(localizer["NameTooLong"], max);
}
