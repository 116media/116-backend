using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a content category (e.g., "Artist Profile", "116 Le Focus", "Chronique Sale").
/// Every article and video must belong to exactly one category. Categories determine whether
/// content is free or paid, and carry their own pricing configuration.
/// </summary>
public class CategoryEntity : Aggregate<Guid>
{
    /// <summary>
    /// The unique identifier of the content type this category belongs to (e.g., "Article", "Video").
    /// </summary>
    public Guid ContentTypeId { get; private set; }

    /// <summary>
    /// Display name of the category (e.g., "Artist Profile", "116 Le Focus").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCategoryNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug for the category (e.g., "artist-profile", "116-le-focus").
    /// Must be unique across all categories.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCategorySlugLength)]
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// Human-readable description of the category.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCategoryDescriptionLength)]
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Indicates whether content in this category is free (no payment required).
    /// When true, the payment flow is skipped entirely.
    /// </summary>
    public bool IsFree { get; private set; }

    /// <summary>
    /// Indicates whether this category is currently active and available for content creation.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// The content type this category is classified under.
    /// </summary>
    public ContentTypeEntity ContentType { get; private set; } = null!;

    /// <summary>
    /// The pricing tiers configured for this category.
    /// </summary>
    public ICollection<CategoryPricingEntity> Pricing { get; } = new List<CategoryPricingEntity>();

    /// <summary>
    /// Package slots that reference this category.
    /// </summary>
    public ICollection<PackageSlotEntity> PackageSlots { get; } = new List<PackageSlotEntity>();

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private CategoryEntity() { }

    /// <summary>
    /// Creates a new category entity.
    /// </summary>
    /// <param name="id">The unique identifier for the category.</param>
    /// <param name="contentTypeId">The identifier of the associated content type.</param>
    /// <param name="name">The display name of the category.</param>
    /// <param name="slug">The URL-safe slug for the category.</param>
    /// <param name="description">The description of the category.</param>
    /// <param name="isFree">Whether content in this category is free.</param>
    /// <returns>A new <see cref="CategoryEntity" /> instance.</returns>
    public static CategoryEntity Create(
        Guid id,
        Guid contentTypeId,
        string name,
        string slug,
        string description,
        bool isFree
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw CategoryErrors.NameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw CategoryErrors.SlugRequired();
        }

        return new CategoryEntity
        {
            Id = id,
            ContentTypeId = contentTypeId,
            Name = name,
            Slug = slug,
            Description = description,
            IsFree = isFree,
            IsActive = true,
        };
    }

    /// <summary>
    /// Updates the category's display name, slug, and description.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="slug">The new URL-safe slug.</param>
    /// <param name="description">The new description.</param>
    public void Update(string name, string slug, string description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw CategoryErrors.NameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw CategoryErrors.SlugRequired();
        }

        Name = name;
        Slug = slug;
        Description = description;
    }

    /// <summary>
    /// Guards that this category is eligible for use on a commissioned order item.
    /// A category is commissionable when it is active and not free.
    /// </summary>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">
    /// Thrown when the category is inactive or free, surfaced as a not-found error to avoid leaking state.
    /// </exception>
    public void EnsureCommissionable()
    {
        if (!IsActive || IsFree)
        {
            throw CategoryErrors.NotFound(id: Id);
        }
    }

    /// <summary>
    /// Activates the category, making it selectable for content creation and orders.
    /// </summary>
    /// <returns>True if the category was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        return true;
    }

    /// <summary>
    /// Deactivates the category, preventing it from being used in new content or orders.
    /// </summary>
    /// <returns>True if the category was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        return true;
    }
}
