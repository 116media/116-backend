using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Category</c> and <c>CategoryPricing</c> domains.
/// Covers conflict situations and validation failures related to category operations.
/// </summary>
public class CategoryErrorMessage(IStringLocalizer<CategoryErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a category with the given slug already exists.
    /// </summary>
    /// <param name="slug">The category slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a category with the specified slug already exists.
    /// </returns>
    public string AlreadyExists(string slug) => string.Format(localizer["AlreadyExists"], slug);

    /// <summary>
    /// Gets an error message for when a category is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the category is already active.
    /// </returns>
    public string AlreadyActive() => localizer["AlreadyActive"];

    /// <summary>
    /// Gets an error message for when a category is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the category is already inactive.
    /// </returns>
    public string AlreadyInactive() => localizer["AlreadyInactive"];

    /// <summary>
    /// Gets an error message for when a category name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the category name is required.
    /// </returns>
    public string NameRequired() => localizer["NameRequired"];

    /// <summary>
    /// Gets an error message for when a category slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the category slug is required.
    /// </returns>
    public string SlugRequired() => localizer["SlugRequired"];

    /// <summary>
    /// Gets an error message for when a pricing tier is already configured for the category.
    /// </summary>
    /// <returns>
    /// An error message indicating that this pricing tier combination already exists for the category.
    /// </returns>
    public string PricingAlreadyExists() => localizer["PricingAlreadyExists"];

    /// <summary>
    /// Gets an error message for when a price must be zero or greater.
    /// </summary>
    /// <returns>
    /// An error message indicating that the price must be a non-negative value.
    /// </returns>
    public string PriceMustBeNonNegative() => localizer["PriceMustBeNonNegative"];

    /// <summary>
    /// Gets an error message for when a category name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string NameTooLong(int max) => string.Format(localizer["NameTooLong"], max);

    /// <summary>
    /// Gets an error message for when a category slug exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SlugTooLong(int max) => string.Format(localizer["SlugTooLong"], max);

    /// <summary>
    /// Gets an error message for when a category slug has an invalid format.
    /// </summary>
    public string SlugInvalidFormat() => localizer["SlugInvalidFormat"];

    /// <summary>
    /// Gets an error message for when a category description is required.
    /// </summary>
    public string DescriptionRequired() => localizer["DescriptionRequired"];

    /// <summary>
    /// Gets an error message for when a category description exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string DescriptionTooLong(int max) => string.Format(localizer["DescriptionTooLong"], max);

    /// <summary>
    /// Gets an error message for when attempting to mark an inactive category as exclusive.
    /// </summary>
    public string CannotMakeInactiveExclusive() => localizer["CannotMakeInactiveExclusive"];

    /// <summary>
    /// Gets an error message for when a file is required but not provided.
    /// </summary>
    public string FileRequired() => localizer["FileRequired"];

    /// <summary>
    /// Gets an error message for when a non-video category is set as exclusive.
    /// </summary>
    public string OnlyVideoCategoryCanBeExclusive() => localizer["OnlyVideoCategoryCanBeExclusive"];

    /// <summary>
    /// Gets an error message for when no exclusive category is currently set.
    /// </summary>
    public string NoExclusiveCategoryFound() => localizer["NoExclusiveCategoryFound"];

    /// <summary>
    /// Gets an error message for when an inactive category is pinned to the feed.
    /// </summary>
    public string CannotPinInactiveToFeed() => localizer["CannotPinInactiveToFeed"];

    /// <summary>
    /// Gets an error message for when a non-feedable content type is pinned to the feed.
    /// </summary>
    public string ContentTypeNotFeedable() => localizer["ContentTypeNotFeedable"];

    /// <summary>
    /// Gets an error message for when a category has too few published videos to be pinned.
    /// </summary>
    /// <param name="minimum">The minimum number of published videos required.</param>
    public string NotEnoughVideosToPinToFeed(int minimum) =>
        string.Format(localizer["NotEnoughVideosToPinToFeed"], minimum);

    /// <summary>
    /// Gets an error message for when no default lyrics category has been configured yet.
    /// </summary>
    public string DefaultLyricsCategoryNotConfigured() => localizer["DefaultLyricsCategoryNotConfigured"];
}
