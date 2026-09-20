using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="PackageSlotBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class PackageSlotFactory
{
    /// <summary>
    /// Creates a slot for a package with a specific category.
    /// </summary>
    public static PackageSlotEntity Create(PackageEntity package) => new PackageSlotBuilder(package).Build();

    /// <summary>
    /// Creates a slot for a package with a specific category ID.
    /// </summary>
    public static PackageSlotEntity Create(PackageEntity package, Guid categoryId) =>
        new PackageSlotBuilder(package).WithCategoryId(categoryId).Build();

    /// <summary>
    /// Creates an open slot (no category assigned — client can choose any).
    /// </summary>
    public static PackageSlotEntity CreateOpen(PackageEntity package) =>
        new PackageSlotBuilder(package).WithCategoryId(null).Build();

    /// <summary>
    /// Creates a slot with explicit category, required flag, and quantity.
    /// </summary>
    public static PackageSlotEntity Create(PackageEntity package, Guid? categoryId, bool isRequired, int quantity) =>
        new PackageSlotBuilder(package)
            .WithCategoryId(categoryId)
            .WithIsRequired(isRequired)
            .WithQuantity(quantity)
            .Build();
}
