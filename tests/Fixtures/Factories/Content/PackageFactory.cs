using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="PackageEntity"/> instances in tests.
/// </summary>
public static class PackageFactory
{
    /// <summary>Creates a package with default random values.</summary>
    public static PackageEntity Create() => new PackageBuilder().Build();

    /// <summary>Creates a package with a specific name.</summary>
    public static PackageEntity Create(string name) => new PackageBuilder().WithName(name).Build();

    /// <summary>Creates an inactive package.</summary>
    public static PackageEntity CreateInactive() => new PackageBuilder().AsInactive().Build();

    /// <summary>Creates a package with known default values.</summary>
    public static PackageEntity CreateDefault() =>
        new PackageBuilder()
            .WithName(TestConstants.Content.Package.ValidName)
            .WithFlatPriceUsd(TestConstants.Content.Package.ValidFlatPriceUsd)
            .Build();

    /// <summary>Creates a list of packages with the specified count.</summary>
    public static List<PackageEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
