using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PricingTierEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; PricingTierFactory only names chains three or more tests share.
/// </summary>
public class PricingTierBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id;
    private string _name;
    private string _description = "Default pricing tier description";
    private bool _isActive = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="PricingTierBuilder"/> class with random default values.
    /// </summary>
    public PricingTierBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        string prefix = word.Length > 4 ? word[..4] : word;
        string unique = $"{prefix}{Guid.NewGuid():N}";
        _name = unique[..Math.Min(TestConstants.PricingTier.NameMaxLength, unique.Length)];
    }

    /// <summary>
    /// Sets the pricing tier ID.
    /// </summary>
    public PricingTierBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the pricing tier name.
    /// </summary>
    public PricingTierBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the pricing tier description.
    /// </summary>
    public PricingTierBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Marks the pricing tier as inactive.
    /// </summary>
    public PricingTierBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PricingTierEntity"/> instance.
    /// </summary>
    public PricingTierEntity Build()
    {
        var entity = PricingTierEntity.Create(_id, _name, _description, TestErrorsFactory.CreatePricingTierErrors());

        if (!_isActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
