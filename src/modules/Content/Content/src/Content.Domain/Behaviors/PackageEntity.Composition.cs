namespace _116.Content.Domain.Entities;

/// <summary>
/// Membership behaviour of <see cref="PackageEntity" />. Its state lives in <c>Entities/PackageEntity.cs</c>.
/// </summary>
public partial class PackageEntity
{
    /// <summary>
    /// Adds a slot to this package.
    /// </summary>
    /// <param name="categoryId">The category the slot is fulfilled from, or null for any.</param>
    /// <param name="isRequired">Whether the slot must be filled for the package to be complete.</param>
    /// <param name="quantity">How many items the slot accepts.</param>
    /// <returns>The slot that was added.</returns>
    public PackageSlotEntity AddSlot(Guid? categoryId, bool isRequired, int quantity)
    {
        PackageSlotEntity slot = PackageSlotEntity.Create(
            id: Guid.NewGuid(),
            packageId: Id,
            categoryId: categoryId,
            isRequired: isRequired,
            quantity: quantity
        );

        Slots.Add(slot);

        return slot;
    }

    /// <summary>
    /// Removes a slot from this package, reporting whether one was there.
    /// </summary>
    /// <param name="slotId">The slot to remove.</param>
    /// <returns><c>true</c> if a slot was removed; otherwise <c>false</c>.</returns>
    public bool RemoveSlot(Guid slotId)
    {
        PackageSlotEntity? existing = FindSlot(slotId: slotId);

        if (existing is null)
        {
            return false;
        }

        Slots.Remove(existing);

        return true;
    }

    /// <summary>
    /// Returns this package's slot by its identifier, or null when it belongs to another package.
    /// </summary>
    /// <param name="slotId">The slot to look up.</param>
    /// <returns>The matching slot, or <c>null</c>.</returns>
    public PackageSlotEntity? FindSlot(Guid slotId)
    {
        return Slots.FirstOrDefault(slot => slot.Id == slotId);
    }
}
