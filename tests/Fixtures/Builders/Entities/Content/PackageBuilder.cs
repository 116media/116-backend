using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PackageEntity"/> instances in tests.
/// For test code, prefer using PackageFactory instead of direct Builder usage.
/// </summary>
internal class PackageBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private string _name;
    private string? _description;
    private decimal _flatPriceUsd;
    private bool _isActive = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageBuilder"/> class with random default values.
    /// </summary>
    public PackageBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        _name = word[..Math.Min(TestConstants.Content.Package.NameMaxLength, word.Length)];
        _flatPriceUsd = _faker.Random.Decimal(0, 1000);
    }

    /// <summary>Sets the package ID.</summary>
    public PackageBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the package name.</summary>
    public PackageBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>Sets the package description.</summary>
    public PackageBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>Sets the flat price in USD.</summary>
    public PackageBuilder WithFlatPriceUsd(decimal flatPriceUsd)
    {
        _flatPriceUsd = flatPriceUsd;
        return this;
    }

    /// <summary>Marks the package as inactive.</summary>
    public PackageBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>Builds the <see cref="PackageEntity"/> instance.</summary>
    public PackageEntity Build()
    {
        var entity = PackageEntity.Create(_id, _name, _description, _flatPriceUsd);

        if (!_isActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
