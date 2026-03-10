using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PackageSlotEntity"/> instances in tests.
/// For test code, prefer using PackageSlotFactory instead of direct Builder usage.
/// </summary>
internal class PackageSlotBuilder
{
    private Guid _id;
    private Guid _packageId;
    private Guid? _categoryId;
    private bool _isRequired = true;
    private int _quantity;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageSlotBuilder"/> class with default values.
    /// </summary>
    public PackageSlotBuilder(Guid packageId)
    {
        _id = Guid.NewGuid();
        _packageId = packageId;
        _quantity = TestConstants.Content.PackageSlot.ValidQuantity;
    }

    /// <summary>Sets the slot ID.</summary>
    public PackageSlotBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the category ID (null for open slot).</summary>
    public PackageSlotBuilder WithCategoryId(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>Sets the required flag.</summary>
    public PackageSlotBuilder WithIsRequired(bool isRequired)
    {
        _isRequired = isRequired;
        return this;
    }

    /// <summary>Marks the slot as optional (not required).</summary>
    public PackageSlotBuilder AsOptional()
    {
        _isRequired = false;
        return this;
    }

    /// <summary>Sets the quantity.</summary>
    public PackageSlotBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    /// <summary>Builds the <see cref="PackageSlotEntity"/> instance.</summary>
    public PackageSlotEntity Build()
    {
        return PackageSlotEntity.Create(_id, _packageId, _categoryId, _isRequired, _quantity);
    }
}
