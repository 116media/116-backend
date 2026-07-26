using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Category domain error factory providing simple, readable exception creation.
/// Usage: CategoryErrors.AlreadyExists(slug) or CategoryErrors.NotFound(id)
/// </summary>
public class CategoryErrors(CategoryErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public CategoryErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a category with the given slug already exists.
    /// </summary>
    public ConflictException AlreadyExists(string slug)
    {
        return new ConflictException(i18n.AlreadyExists(slug: slug));
    }

    /// <summary>
    /// Throws when a category is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Category", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a category is already active.
    /// </summary>
    public ConflictException AlreadyActive()
    {
        return new ConflictException(i18n.AlreadyActive());
    }

    /// <summary>
    /// Throws when a category is already inactive.
    /// </summary>
    public ConflictException AlreadyInactive()
    {
        return new ConflictException(i18n.AlreadyInactive());
    }

    /// <summary>
    /// Throws when the default free lyrics category — the seeded fallback every
    /// community-originated lyrics record (submission approval or verified-artist upload) is
    /// filed under — has not been configured yet. A real, actionable setup error rather than a
    /// silent failure.
    /// </summary>
    public InternalServerException DefaultLyricsCategoryNotConfigured()
    {
        return new InternalServerException(i18n.DefaultLyricsCategoryNotConfigured());
    }

    /// <summary>
    /// Throws when a category name is required but not provided.
    /// </summary>
    public BadRequestException NameRequired()
    {
        return new BadRequestException(i18n.NameRequired());
    }

    /// <summary>
    /// Throws when a category slug is required but not provided.
    /// </summary>
    public BadRequestException SlugRequired()
    {
        return new BadRequestException(i18n.SlugRequired());
    }

    /// <summary>
    /// Throws when the pricing tier is already configured for the category.
    /// </summary>
    public ConflictException PricingAlreadyExists()
    {
        return new ConflictException(i18n.PricingAlreadyExists());
    }

    /// <summary>
    /// Throws when a category pricing row is not found.
    /// </summary>
    public NotFoundException PricingNotFound(Guid categoryId, Guid tierId)
    {
        return new NotFoundException("CategoryPricing", "categoryId+tierId", keyValue: $"{categoryId}/{tierId}");
    }

    /// <summary>
    /// Throws when a price is negative.
    /// </summary>
    public BadRequestException PriceMustBeNonNegative()
    {
        return new BadRequestException(i18n.PriceMustBeNonNegative());
    }

    /// <summary>
    /// Throws when attempting to mark an inactive category as exclusive.
    /// </summary>
    public BadRequestException CannotMakeInactiveExclusive()
    {
        return new BadRequestException(i18n.CannotMakeInactiveExclusive());
    }

    /// <summary>
    /// Throws when a non-video category is set as exclusive.
    /// </summary>
    public BadRequestException OnlyVideoCategoryCanBeExclusive()
    {
        return new BadRequestException(i18n.OnlyVideoCategoryCanBeExclusive());
    }

    /// <summary>
    /// Throws when attempting to mark an inactive category as the default for lyrics pages.
    /// </summary>
    public BadRequestException CannotMakeInactiveDefaultForLyrics()
    {
        return new BadRequestException(i18n.CannotMakeInactiveDefaultForLyrics());
    }

    /// <summary>
    /// Throws when a category outside the Lyrics content type is set as the lyrics default.
    /// </summary>
    public BadRequestException OnlyLyricsCategoryCanBeDefault()
    {
        return new BadRequestException(i18n.OnlyLyricsCategoryCanBeDefault());
    }

    /// <summary>
    /// Throws when no exclusive category is currently set.
    /// </summary>
    public NotFoundException NoExclusiveCategoryFound()
    {
        return new NotFoundException(i18n.NoExclusiveCategoryFound());
    }

    /// <summary>
    /// Throws when attempting to pin an inactive category to the content feed.
    /// </summary>
    public BadRequestException CannotPinInactiveToFeed()
    {
        return new BadRequestException(i18n.CannotPinInactiveToFeed());
    }

    /// <summary>
    /// Throws when attempting to pin a category whose content type cannot appear
    /// in a feed (only Video and Article are feedable).
    /// </summary>
    public BadRequestException ContentTypeNotFeedable()
    {
        return new BadRequestException(i18n.ContentTypeNotFeedable());
    }

    /// <summary>
    /// Throws when attempting to pin a category that does not have the minimum number
    /// of published videos required to appear as a feed section.
    /// </summary>
    /// <param name="minimum">The minimum number of published videos required.</param>
    public BadRequestException NotEnoughVideosToPinToFeed(int minimum)
    {
        return new BadRequestException(i18n.NotEnoughVideosToPinToFeed(minimum));
    }
}
