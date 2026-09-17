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
    private readonly PackageEntity _package;
    private Guid? _categoryId;
    private bool _isRequired = true;
    private int _quantity;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageSlotBuilder"/> class with default values.
    /// </summary>
    public PackageSlotBuilder(PackageEntity package)
    {
        _package = package;
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
        _categoryId = category.Id;
        _categoryId = category.Id;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PackageSlotEntity"/> instance.
    /// </summary>
    public PackageSlotEntity Build()
    {
        PackageSlotEntity slot = _package.AddSlot(
            categoryId: _categoryId,
            isRequired: _isRequired,
            quantity: _quantity
        );

        return slot;
    }
}
