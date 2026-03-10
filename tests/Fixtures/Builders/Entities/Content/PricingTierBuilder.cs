using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PricingTierEntity"/> instances in tests.
/// For test code, prefer using PricingTierFactory instead of direct Builder usage.
/// </summary>
internal class PricingTierBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private string _name;
    private string? _description;
    private bool _isActive = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="PricingTierBuilder"/> class with random default values.
    /// </summary>
    public PricingTierBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        _name = word[..Math.Min(TestConstants.Content.PricingTier.NameMaxLength, word.Length)];
    }

    /// <summary>Sets the pricing tier ID.</summary>
    public PricingTierBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the pricing tier name.</summary>
    public PricingTierBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>Sets the pricing tier description.</summary>
    public PricingTierBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>Marks the pricing tier as inactive.</summary>
    public PricingTierBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>Builds the <see cref="PricingTierEntity"/> instance.</summary>
    public PricingTierEntity Build()
    {
        var entity = PricingTierEntity.Create(_id, _name, _description);

        if (!_isActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
