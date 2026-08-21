using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PackageSlotEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; PackageSlotFactory only names chains three or more tests share.
/// </summary>
public class PackageSlotBuilder
{
    private Guid _id;
    private Guid _packageId;
    private Guid? _categoryId;
    private bool _isRequired = true;
    private int _quantity;
    private CategoryEntity? _category;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageSlotBuilder"/> class with default values.
    /// </summary>
    public PackageSlotBuilder(Guid packageId)
    {
        _id = Guid.NewGuid();
        _packageId = packageId;
        _quantity = TestConstants.PackageSlot.ValidQuantity;
    }

    /// <summary>
    /// Sets the category ID (null for open slot).
    /// </summary>
    public PackageSlotBuilder WithCategoryId(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>
    /// Sets the required flag.
    /// </summary>
    public PackageSlotBuilder WithIsRequired(bool isRequired)
    {
        _isRequired = isRequired;
        return this;
    }

    /// <summary>
    /// Sets the quantity.
    /// </summary>
    public PackageSlotBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    /// <summary>
    /// Attaches the Category navigation EF Core populates through <c>.Include(s =&gt; s.Category)</c>,
    /// and points the foreign key at the same category.
    /// </summary>
    public PackageSlotBuilder WithCategory(CategoryEntity category)
    {
        _category = category;
        _categoryId = category.Id;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PackageSlotEntity"/> instance.
    /// </summary>
    public PackageSlotEntity Build()
    {
        PackageSlotEntity slot = PackageSlotEntity.Create(_id, _packageId, _categoryId, _isRequired, _quantity);

        if (_category is not null)
        {
            typeof(PackageSlotEntity)
                .GetProperty(nameof(PackageSlotEntity.Category), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(slot, _category);
        }

        return slot;
    }
}
