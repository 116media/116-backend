using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Category domain error factory providing simple, readable exception creation.
/// Usage: CategoryErrors.AlreadyExists(slug) or CategoryErrors.NotFound(id)
/// </summary>
public static class CategoryErrors
{
    /// <summary>Throws when a category with the given slug already exists.</summary>
    public static ConflictException AlreadyExists(string slug)
    {
        return new ConflictException(CategoryErrorMessage.AlreadyExists(slug: slug));
    }

    /// <summary>Throws when a category is not found by its identifier.</summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Category", "id", keyValue: id);
    }

    /// <summary>Throws when a category is already active.</summary>
    public static ConflictException AlreadyActive()
    {
        return new ConflictException(CategoryErrorMessage.AlreadyActive());
    }

    /// <summary>Throws when a category is already inactive.</summary>
    public static ConflictException AlreadyInactive()
    {
        return new ConflictException(CategoryErrorMessage.AlreadyInactive());
    }

    /// <summary>Throws when a category name is required but not provided.</summary>
    public static BadRequestException NameRequired()
    {
        return new BadRequestException(CategoryErrorMessage.NameRequired());
    }

    /// <summary>Throws when a category slug is required but not provided.</summary>
    public static BadRequestException SlugRequired()
    {
        return new BadRequestException(CategoryErrorMessage.SlugRequired());
    }

    /// <summary>Throws when the pricing tier is already configured for the category.</summary>
    public static ConflictException PricingAlreadyExists()
    {
        return new ConflictException(CategoryErrorMessage.PricingAlreadyExists());
    }

    /// <summary>Throws when a category pricing row is not found.</summary>
    public static NotFoundException PricingNotFound(Guid categoryId, Guid tierId)
    {
        return new NotFoundException("CategoryPricing", "categoryId+tierId", keyValue: $"{categoryId}/{tierId}");
    }

    /// <summary>Throws when a price is negative.</summary>
    public static BadRequestException PriceMustBeNonNegative()
    {
        return new BadRequestException(CategoryErrorMessage.PriceMustBeNonNegative());
    }
}
