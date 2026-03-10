using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="PackageSlotEntity"/> instances in tests.
/// </summary>
public static class PackageSlotFactory
{
    /// <summary>Creates a slot for a package with a specific category.</summary>
    public static PackageSlotEntity Create(Guid packageId) => new PackageSlotBuilder(packageId).Build();

    /// <summary>Creates a slot for a package with a specific category ID.</summary>
    public static PackageSlotEntity Create(Guid packageId, Guid categoryId) =>
        new PackageSlotBuilder(packageId).WithCategoryId(categoryId).Build();

    /// <summary>Creates an open slot (no category assigned — client can choose any).</summary>
    public static PackageSlotEntity CreateOpen(Guid packageId) =>
        new PackageSlotBuilder(packageId).WithCategoryId(null).Build();

    /// <summary>Creates a required slot.</summary>
    public static PackageSlotEntity CreateRequired(Guid packageId) =>
        new PackageSlotBuilder(packageId).WithIsRequired(true).Build();

    /// <summary>Creates an optional (non-required) slot.</summary>
    public static PackageSlotEntity CreateOptional(Guid packageId) =>
        new PackageSlotBuilder(packageId).AsOptional().Build();
}
