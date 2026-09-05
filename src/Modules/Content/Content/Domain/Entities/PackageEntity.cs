using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a named bundle deal offered to clients who want multiple content pieces
/// (e.g., "Artist Starter Pack: 1 × Artist Profile + 1 × 116 Interview").
/// The price is derived from the required slots' category tier prices, not manually set.
/// </summary>
public class PackageEntity : Aggregate<Guid>
{
    /// <summary>
    /// Display name of the package (e.g., "Artist Starter Pack").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxPackageNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Description of what the package includes.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxPackageDescriptionLength)]
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Indicates whether this package is active and available for new orders.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// The content slots that define the composition of this package.
    /// </summary>
    public ICollection<PackageSlotEntity> Slots { get; } = new List<PackageSlotEntity>();

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private PackageEntity() { }

    /// <summary>
    /// Creates a new package entity.
    /// </summary>
    /// <param name="id">The unique identifier for the package.</param>
    /// <param name="name">The display name of the package.</param>
    /// <param name="description">The description of the package.</param>
    /// <returns>A new <see cref="PackageEntity" /> instance.</returns>
    public static PackageEntity Create(Guid id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.PackageNameRequired);
        }

        return new PackageEntity
        {
            Id = id,
            Name = name,
            Description = description,
            IsActive = true,
        };
    }

    /// <summary>
    /// Activates the package, making it available for new orders.
    /// </summary>
    /// <returns>True if the package was activated, false if already active.</returns>
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
    /// Deactivates the package, removing it from available bundles for new orders.
    /// </summary>
    /// <returns>True if the package was deactivated, false if already inactive.</returns>
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
