using _116.Content.Application.Shared.Errors;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a content slot within a package, defining what content must be delivered
/// to fulfill the package (e.g., "1 × Artist Profile video", or an open slot where the client picks any category).
/// </summary>
public class PackageSlotEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identifier of the package this slot belongs to.
    /// </summary>
    public Guid PackageId { get; private set; }

    /// <summary>
    /// The identifier of the specific category for this slot.
    /// When null, the slot is open — the client may choose any category.
    /// </summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>
    /// Indicates whether this slot must be fulfilled for the package to be considered complete.
    /// Optional slots are bonus entries the client can claim.
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// The number of content pieces required for this slot.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// The package this slot belongs to.
    /// </summary>
    public PackageEntity Package { get; private set; } = null!;

    /// <summary>
    /// The specific category for this slot, or null for an open slot.
    /// </summary>
    public CategoryEntity? Category { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private PackageSlotEntity() { }

    /// <summary>
    /// Creates a new package slot entity.
    /// </summary>
    /// <param name="id">The unique identifier for the slot.</param>
    /// <param name="packageId">The identifier of the parent package.</param>
    /// <param name="categoryId">The identifier of the category, or null for an open slot.</param>
    /// <param name="isRequired">Whether this slot is required to fulfill the package.</param>
    /// <param name="quantity">The number of content pieces required (must be > 0).</param>
    /// <returns>A new <see cref="PackageSlotEntity" /> instance.</returns>
    public static PackageSlotEntity Create(Guid id, Guid packageId, Guid? categoryId, bool isRequired, int quantity)
    {
        if (quantity <= 0)
        {
            throw PackageErrors.SlotQuantityMustBePositive();
        }

        return new PackageSlotEntity
        {
            Id = id,
            PackageId = packageId,
            CategoryId = categoryId,
            IsRequired = isRequired,
            Quantity = quantity,
        };
    }
}
